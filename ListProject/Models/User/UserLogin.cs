using System.ComponentModel.DataAnnotations;

namespace ListProject.Models.User
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
