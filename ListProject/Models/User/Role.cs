using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ListProject.Models.User
{
    public class Role : IdentityRole<Guid>
    {
        [Key]
        public override Guid Id { get; set; }
        public override string Name { get; set; }
        public bool? IsDeleted { get; set; }
        public int? Status { get; set; }
    }
}
