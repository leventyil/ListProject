using ListProject.Models;
using ListProject.Models.User;
using ListProject.Repository;
using ListProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ListProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<WantToWatchList> _wantToWatchRepository;
        private readonly IRepository<WatchedList> _watcedRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public MovieController(IRepository<Movie> movieRepository, IRepository<Category> categoryRepository, SignInManager<User> _signInManager, UserManager<User> userManager, RoleManager<Role> _roleManager, IRepository<WantToWatchList> wantToWatchRepository, IRepository<WatchedList> watchedRepository)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
            this._userManager = userManager;
            this._roleManager = _roleManager;
            _wantToWatchRepository = wantToWatchRepository;
            _watcedRepository = watchedRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var movies = _movieRepository.GetAll(m => m.IsDeleted != true).ToList();
            var categories = _categoryRepository.GetAll().Where(c => c.IsDeleted != true).ToList();
            var currentUser = await _userManager.GetUserAsync(User);
            var wList = new List<WatchedList>();
            var wtwList = new List<WantToWatchList>();

            if(currentUser != null)
            {
                wtwList = (from w in _wantToWatchRepository.WantToWatchList() where w.UserId == currentUser.Id select w).ToList();
                wList = (from w in _watcedRepository.WatchedList() where w.UserId == currentUser.Id select w).ToList();
            }       

            var all_parameters = new MovieViewModel();
            all_parameters.Movies = movies;
            all_parameters.Categories = categories;
            all_parameters.WatchedList = wList;
            all_parameters.WantToWatchList = wtwList;

            return View(all_parameters);
        }

        [HttpGet]
        public async Task<IActionResult> InsertMovie()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> InsertMovie(Movie movie, IFormFile image)
        {
            var res = new ApiResult();

            var currentUser = await _userManager.GetUserAsync(User);
            var category = await _categoryRepository.FindAsync(movie.CategoryId);

            try
            {
                movie.Id = Guid.NewGuid();
                if(currentUser != null)
                {
                    movie.CreatedBy = await _userManager.GetUserNameAsync(currentUser);
                }
                movie.IsDeleted = false;
                movie.CreatedDate = DateTime.Now;
                movie.CategoryName = category.Name;

                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    movie.ImageData = memoryStream.ToArray();
                }

                await _movieRepository.AddAsync(movie);
                await _movieRepository.SaveAsync();

                res.IsSuccess = true;
                res.Message = "Movie added succesfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occured.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMovie(Guid movieId)
        {
            var res = new ApiResult();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                var movie = _movieRepository.Find(movieId);

                if (movie != null)
                {
                    if (currentUser != null)
                    {
                        movie.DeletedBy = await _userManager.GetUserNameAsync(currentUser);                    
                    }
                    movie.DeletedDate = DateTime.Now;
                    movie.IsDeleted = true;

                    _movieRepository.Update(movie);
                    await _movieRepository.SaveAsync();

                    res.IsSuccess = true;
                    res.Message = "Movie deleted successfully.";
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "Movie not found.";
                }
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occured.";
            }

            return Json(res);
        }

        [HttpGet]
        public IActionResult Categories()
        {
            var categories = _categoryRepository.GetAll().Where(c => c.IsDeleted != true).ToList();
            return View(categories);
        }

        [HttpPost]
        public async Task<JsonResult> AddCategory(Category model)
        {
            var res = new ApiResult();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                model.Id = Guid.NewGuid();
                model.IsDeleted = false;
                model.CreatedDate = DateTime.Now;
                if (currentUser != null)
                {
                    model.CreatedBy = await _userManager.GetUserNameAsync(currentUser);
                }             

                await _categoryRepository.AddAsync(model);
                await _categoryRepository.SaveAsync();

                res.IsSuccess = true;
                res.Message = "Category added successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while adding the category.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateCategory(Guid categoryId, string newCategoryName)
        {
            var res = new ApiResult();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                var category = await _categoryRepository.FindAsync(categoryId);
                if (category != null)
                {
                    category.Name = newCategoryName;
                    if (currentUser != null)
                    {
                        category.ModifiedBy = await _userManager.GetUserNameAsync(currentUser);
                    }
                    category.ModifiedDate = DateTime.Now;

                    _categoryRepository.Update(category);
                    await _categoryRepository.SaveAsync();
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "An error occurred while finding the category.";

                    return Json(res);
                }


                res.IsSuccess = true;
                res.Message = "Category updated successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while updating the category.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteCategory(Guid categoryId)
        {
            var res = new ApiResult();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                var category = await _categoryRepository.FindAsync(categoryId);
                if (category != null)
                {
                    category.IsDeleted = true;
                    category.DeletedDate = DateTime.Now;
                    if (currentUser != null)
                    {
                        category.DeletedBy = await _userManager.GetUserNameAsync(currentUser);                     
                    }

                    _categoryRepository.Update(category);
                    await _categoryRepository.SaveAsync();
                }
                else
                {
                    res.IsSuccess = false;
                    res.Message = "An error occurred while finding the category.";

                    return Json(res);
                }


                res.IsSuccess = true;
                res.Message = "Category deleted successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occurred while deleting the category.";
            }

            return Json(res);
        }

        public async Task<IActionResult> WantToWatchList(string userId)
        {          
            List<Guid> movieIds = new List<Guid>();
            var movies = new List<Movie>();
            var listOwnerUser = new User();
            var wList = new List<WatchedList>();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                listOwnerUser = await _userManager.FindByIdAsync(userId);
                if(listOwnerUser != null)
                {
                    movieIds = (from wtw in _wantToWatchRepository.WantToWatchList() where wtw.UserId == listOwnerUser.Id select wtw.MovieId).ToList();
                    wList = (from w in _watcedRepository.WatchedList() where w.UserId == currentUser.Id select w).ToList();
                }
                if (currentUser != null)
                {
                    wList = (from w in _watcedRepository.WatchedList() where w.UserId == currentUser.Id select w).ToList();
                }

                if (movieIds != null)
                {
                    foreach(var movieId in movieIds)
                    {
                        var movie = await _movieRepository.FindAsync(movieId);
                        if(movie != null && movie.IsDeleted != true)
                        {
                            movies.Add(movie);
                        }
                    }
                }
            }
            catch 
            {

            }

            var all_parameters = new WantToWatchListViewModel();
            all_parameters.User = listOwnerUser;
            all_parameters.Movies = movies;
            all_parameters.WatchedList = wList;

            return View(all_parameters);
        }

        public async Task<IActionResult> WatchedList(string userId)
        {
            List<Guid> movieIds = new List<Guid>();
            var movies = new List<Movie>();
            var listOwnerUser = new User();
            var wtwList = new List<WantToWatchList>();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                listOwnerUser = await _userManager.FindByIdAsync(userId);
                if (listOwnerUser != null)
                {
                    movieIds = (from w in _watcedRepository.WatchedList() where w.UserId == listOwnerUser.Id select w.MovieId).ToList();
                }
                if (currentUser != null)
                {
                    wtwList = (from w in _wantToWatchRepository.WantToWatchList() where w.UserId == currentUser.Id select w).ToList();
                }

                if (movieIds != null)
                {
                    foreach (var movieId in movieIds)
                    {
                        var movie = await _movieRepository.FindAsync(movieId);
                        if(movie != null && movie.IsDeleted != true)
                        {
                            movies.Add(movie);
                        }
                    }
                }
            }
            catch
            {

            }

            var all_parameters = new WatchedListViewModel();
            all_parameters.User = listOwnerUser;
            all_parameters.Movies = movies;
            all_parameters.WantToWatchList = wtwList;

            return View(all_parameters);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWatchedList(Guid movieId)
        {
            var res = new ApiResult();
            var model = new WatchedList();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                if (currentUser != null)
                {
                    var record = (from w in _watcedRepository.WatchedList() where w.MovieId == movieId && w.UserId == currentUser.Id select w).ToList();
                    if (record.Count != 0 && record != null)
                    {
                        res.IsSuccess = false;
                        res.Message = "This movie is already on the list.";

                        return Json(res);
                    }
                    model.UserId = currentUser.Id;
                }
                model.MovieId = movieId;

                await _watcedRepository.AddAsync(model);
                await _watcedRepository.SaveAsync();

                res.IsSuccess = true;
                res.Message = "Movie added to the watched list successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occured.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWantToWatchList(Guid movieId)
        {
            var res = new ApiResult();
            var model = new WantToWatchList();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                if (currentUser != null)
                {
                    var record = (from w in _wantToWatchRepository.WantToWatchList() where w.MovieId == movieId && w.UserId == currentUser.Id select w).ToList();
                    if(record.Count != 0 && record != null)
                    {
                        res.IsSuccess = false;
                        res.Message = "This movie is already on the list.";

                        return Json(res);
                    }
                    model.UserId = currentUser.Id;
                }
                model.MovieId = movieId;

                await _wantToWatchRepository.AddAsync(model);
                await _wantToWatchRepository.SaveAsync();

                res.IsSuccess = true;
                res.Message = "Movie added to the want to watch list successfully.";
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occured.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMovieFromWTW(Guid movieId)
        {
            var res = new ApiResult();
            var model = new WantToWatchList();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                if (currentUser != null)
                {
                    var record = (from w in _wantToWatchRepository.WantToWatchList() where w.MovieId == movieId && w.UserId == currentUser.Id select w).Single();
                    if (record != null)
                    {
                        _wantToWatchRepository.Delete(record);
                        await _wantToWatchRepository.SaveAsync();

                        res.IsSuccess = true;
                        res.Message = "Movie removed from want to watch list successfully.";
                    }
                }                   
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occured.";
            }

            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMovieFromW(Guid movieId)
        {
            var res = new ApiResult();
            var model = new WantToWatchList();
            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                if (currentUser != null)
                {
                    var record = (from w in _watcedRepository.WatchedList() where w.MovieId == movieId && w.UserId == currentUser.Id select w).Single();
                    if (record != null)
                    {
                        _watcedRepository.Delete(record);
                        await _wantToWatchRepository.SaveAsync();

                        res.IsSuccess = true;
                        res.Message = "Movie removed from watched list successfully.";
                    }
                }
            }
            catch
            {
                res.IsSuccess = false;
                res.Message = "An error occured.";
            }

            return Json(res);
        }
    }
}
