using ListProject.Models;

namespace ListProject.ViewModels
{
    public class MovieViewModel
    {
        public List<Movie> Movies { get; set; }
        public List<Category> Categories { get; set; }
        public List<WantToWatchList>? WantToWatchList { get; set; }
        public List<WatchedList>? WatchedList { get; set; }
    }
}
