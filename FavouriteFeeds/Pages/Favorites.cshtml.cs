using System.Collections.Generic;
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
        private const string FavouritesCookieName = "Favourites";

        public FavouritesModel(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<RssFeed> FavoriteFeeds { get; set; } = new List<RssFeed>();

        public void OnGet()
        {
            var favouritesJson = _httpContextAccessor.HttpContext.Request.Cookies[FavouritesCookieName];
            FavoriteFeeds = string.IsNullOrEmpty(favouritesJson) ? new List<RssFeed>() : JsonConvert.DeserializeObject<List<RssFeed>>(favouritesJson);

            Debug.WriteLine("FavoriteFeeds Count: " + FavoriteFeeds.Count);
        }

        public IActionResult OnPostDeleteStar(string xmlUrl, string htmlUrl, string RssFeedTitle)
        {
            var favouritesJson = _httpContextAccessor.HttpContext.Request.Cookies[FavouritesCookieName];
            var favourites = string.IsNullOrEmpty(favouritesJson) ? new List<RssFeed>() : JsonConvert.DeserializeObject<List<RssFeed>>(favouritesJson);

            var RssFeedToRemove = favourites.FirstOrDefault(f => f.XmlUrl == xmlUrl && f.HtmlUrl == htmlUrl && f.FeedTitle == RssFeedTitle);
            if (RssFeedToRemove != null)
            {
                favourites.Remove(RssFeedToRemove);
            }

            favouritesJson = JsonConvert.SerializeObject(favourites);
            _httpContextAccessor.HttpContext.Response.Cookies.Append(FavouritesCookieName, favouritesJson);

            return RedirectToPage("/Favourites");
        }
    }

    public class RssFeed
    {
        public string XmlUrl { get; set; }
        public string HtmlUrl { get; set; }
        public string FeedTitle { get; set; }
    }
}
