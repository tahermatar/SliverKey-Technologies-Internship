using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FavouriteFeeds.Pages.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToggleFavoriteModel : ControllerBase
    {
        private readonly ILogger<ToggleFavoriteModel> _logger;
        private const string FavouritesCookieName = "Favourites";
        public bool Success { get; set; }
        public bool IsFavorite { get; set; }

        public ToggleFavoriteModel(ILogger<ToggleFavoriteModel> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult ToggleFavorite([FromBody] FavoriteRequest request)
        {
            try
            {
                var favoriteFeedsJson = Request.Cookies[FavouritesCookieName];
                var favoriteFeeds = string.IsNullOrEmpty(favoriteFeedsJson)
                    ? new List<RssFeed>()
                    : JsonConvert.DeserializeObject<List<RssFeed>>(favoriteFeedsJson);

                var favoriteFeed = favoriteFeeds.FirstOrDefault(f => f.Link == request.Link);
                if (favoriteFeed != null)
                {
                    favoriteFeeds.Remove(favoriteFeed);
                    favoriteFeed.IsFavorite = false;
                }
                else
                {
                    favoriteFeed = new RssFeed { Link = request.Link, IsFavorite = true };
                    favoriteFeeds.Add(favoriteFeed);
                }

                var serializedFavoriteFeeds = JsonConvert.SerializeObject(favoriteFeeds);
                Response.Cookies.Append(FavouritesCookieName, serializedFavoriteFeeds);

                return Ok(new { success = true, isFavorite = favoriteFeed.IsFavorite });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling favorite status.");
                return StatusCode(500, new { success = false, isFavorite = false });
            }
        }

        public class FavoriteRequest
        {
            public string Link { get; set; }
            public string Title { get; set; }
        }
    }
}
