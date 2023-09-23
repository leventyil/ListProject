using ListProject.Context;
using ListProject.Models;
using ListProject.Models.User;
using ListProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Data;

namespace ListProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(AppDbContext context, UserManager<User> userManager, SignInManager<User> _signInManager, RoleManager<Role> _roleManager)
        {
            this._context = context;
            this._userManager = userManager;
            this._roleManager = _roleManager;
            this._signInManager = _signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Movie");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(UserLogin model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login");
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Movie");
                    }
                    else
                    {
                        ViewBag.PasswordError = "Şifreniz yanlış.";
                        ViewBag.Password = false;
                        return View("Login");
                    }
                }
                else
                {
                    ViewBag.UserError = "Böyle bir kullanıcı mevcut değil.";
                    ViewBag.User = false;
                    return View("Login");
                }             
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Movie");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(UserRegister model)
        {
            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    Email = model.Email,
                    UserName = model.Email,
                    CreateTime = DateTime.Now,
                    Status = 1,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Roles()
        {
            var roles = (from r in _roleManager.Roles where r.IsDeleted != true && r.Status != -1 select r).ToList();
            return View(roles);
        }

        [HttpPost]
        public async Task<JsonResult> AddRole(Role model)
        {
            var res = new ApiResult();

            try
            {
                model.Id = Guid.NewGuid();
                model.Status = 1;
                model.IsDeleted = false;

                var result = await _roleManager.CreateAsync(model);

                res.IsSuccess = true;
                res.Message = "Role added successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while adding the role.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateRole(string oldRoleName, string newRoleName)
        {
            var res = new ApiResult();

            try
            {
                var role = await _roleManager.FindByNameAsync(oldRoleName);
                if (role != null)
                {
                    role.Name = newRoleName;

                    await _roleManager.UpdateAsync(role);
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "An error occurred while finding the role.";

                    return Json(res);
                }
                    

                res.IsSuccess = true;
                res.Message = "Role updated successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while updating the role.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteRole(Role model)
        {
            var res = new ApiResult();

            try
            {
                var role = await _roleManager.FindByNameAsync(model.Name);
                if(role != null)
                {
                    role.IsDeleted = true;
                    role.Status = -1;

                    await _roleManager.UpdateAsync(role);
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "An error occurred while finding the role.";

                    return Json(res);
                }
                

                res.IsSuccess = true;
                res.Message = "Role deleted successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while deleting the role.";
            }

            return Json(res);
        }

        public async Task<IActionResult> Role(string roleName)
        {
            var role = new Role();
            IList<User> roleUsers = new List<User>();
            var allUsers = new List<User>();

            try
            {
                role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    roleUsers = await _userManager.GetUsersInRoleAsync(role.Name);
                }

                allUsers = (from u in _userManager.Users where u.IsDeleted != true && u.Status != -1 select u).ToList();

                var usersToRemove = from u in allUsers
                                    join ru in roleUsers on u.Email equals ru.Email
                                    select u;

                allUsers = allUsers.Except(usersToRemove).ToList();
            }
            catch
            {
            }

            var all_parameters = new RoleViewModel();
            all_parameters.Users = roleUsers;
            all_parameters.Role = role;
            all_parameters.AllUsers = allUsers;

            return View(all_parameters);
        }

        [HttpPost]
        public async Task<JsonResult> RemoveFromRole(string userName, string roleName)
        {
            var res = new ApiResult();

            try
            {
                var user = await _userManager.FindByEmailAsync(userName);

                if(user != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "An error occurred while removing the role.";

                    return Json(res);
                }

                res.IsSuccess = true;
                res.Message = "Role removed successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while removing the role.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> AddToRole(string email, string roleName)
        {
            var res = new ApiResult();

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "An error occurred while adding user to the role.";

                    return Json(res);
                }

                res.IsSuccess = true;
                res.Message = "User added to the role successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while adding user to the role.";
            }

            return Json(res);
        }
    }
}
