using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FavouriteFeeds.Pages
{
    public class ToggleFavoriteModel : PageModel
    {
        public bool Success { get; set; }
        public bool IsFavorite { get; set; }

        public void OnGet(bool success, bool isFavorite)
        {
            Success = success;
            IsFavorite = isFavorite;
        }
    }
}
