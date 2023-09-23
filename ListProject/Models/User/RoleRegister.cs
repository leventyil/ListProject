using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ListProject.Models.User
{
    public class RoleRegister
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
