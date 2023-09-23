using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ListProject.Models.User
{
    public class User : IdentityUser<Guid>
    {
        [Key]
        public override Guid Id { get; set; }
        public override string? Email { get; set; }
        public int? Status { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
