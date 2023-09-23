using ListProject.Models.User;

namespace ListProject.ViewModels
{
    public class RoleViewModel
    {
        public Role Role { get; set; }
        public IList<User>? Users { get; set; }
        public List<User>? AllUsers { get; set; }
    }
}
