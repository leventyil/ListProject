using Microsoft.EntityFrameworkCore;

namespace ListProject.Models
{
    public class WantToWatchList
    {       
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MovieId { get; set; }
    }
}
