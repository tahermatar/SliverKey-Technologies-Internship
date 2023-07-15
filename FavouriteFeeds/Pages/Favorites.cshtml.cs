using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace FavouriteFeeds.Pages
{
    public class FavouritesModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<IndexModel> _logger;

        private const string FavouritesCookieName = "Favourites";
        public int PageSize { get; set; } = 10;

        public FavouritesModel(IHttpContextAccessor httpContextAccessor, ILogger<IndexModel> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public List<RssFeed> FavoriteFeeds { get; set; } = new List<RssFeed>();

        public async Task<IActionResult> OnGet([FromQuery] int page = 1)
        {
            var favouritesJson = _httpContextAccessor.HttpContext.Request.Cookies[FavouritesCookieName];
            FavoriteFeeds = string.IsNullOrEmpty(favouritesJson) ? new List<RssFeed>() : JsonConvert.DeserializeObject<List<RssFeed>>(favouritesJson);

            var itemCount = FavoriteFeeds.Count;

            var startIndex = (page - 1) * PageSize;
            var endIndex = startIndex + PageSize;

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)itemCount / PageSize);

            return Page();
        }

        public IActionResult OnPostDeleteStar(string link, int page)
        {
            try
            {
                var favouritesJson = _httpContextAccessor.HttpContext.Request.Cookies[FavouritesCookieName];
                var favoriteFeeds = string.IsNullOrEmpty(favouritesJson) ? new List<RssFeed>() : JsonConvert.DeserializeObject<List<RssFeed>>(favouritesJson);

                var favoriteFeed = favoriteFeeds.FirstOrDefault(f => f.Link == link);
                if (favoriteFeed != null)
                {
                    favoriteFeeds.Remove(favoriteFeed);
                    favoriteFeed.IsFavorite = false;
                }
                else
                {
                    favoriteFeed = new RssFeed { Link = link, IsFavorite = true };
                    favoriteFeeds.Add(favoriteFeed);
                }

                var serializedFavoriteFeeds = JsonConvert.SerializeObject(favoriteFeeds);
                _httpContextAccessor.HttpContext.Response.Cookies.Append(FavouritesCookieName, serializedFavoriteFeeds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling favorite status.");
            }

            return RedirectToPage();
        }
    }
}
