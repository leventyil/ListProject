using ListProject.Models.User;
using ListProject.Models;

namespace ListProject.ViewModels
{
    public class WantToWatchListViewModel
    {
        public List<Movie> Movies { get; set; }
        public User User { get; set; }
        public List<WatchedList>? WatchedList { get; set; }

    }
}
