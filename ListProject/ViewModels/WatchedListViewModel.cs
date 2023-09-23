using ListProject.Models;
using ListProject.Models.User;

namespace ListProject.ViewModels
{
    public class WatchedListViewModel
    {
        public List<Movie> Movies { get; set; }
        public User User { get; set; }
        public List<WantToWatchList>? WantToWatchList { get; set; }

    }
}
