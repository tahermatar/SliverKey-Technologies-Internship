using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FavouriteFeeds.Pages.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndexController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexController> _logger;
        private const string FavouritesCookieName = "Favourites";
        public int PageSize { get; set; } = 10;

        public IndexController(IHttpClientFactory httpClientFactory, ILogger<IndexController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFeeds([FromQuery] int page = 1)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");

                if (response.IsSuccessStatusCode)
                {
                    var xmlContent = await response.Content.ReadAsStringAsync();
                    var feedUrls = ParseOpml(xmlContent);

                    var favoriteFeedsJson = Request.Cookies[FavouritesCookieName];
                    var favoriteFeeds = string.IsNullOrEmpty(favoriteFeedsJson)
                        ? new List<RssFeed>()
                        : JsonConvert.DeserializeObject<List<RssFeed>>(favoriteFeedsJson);

                    var paginatedFeedUrls = feedUrls.Skip((page - 1) * PageSize).Take(PageSize);

                    var feedsDetails = new List<RssFeed>();

                    foreach (var feedUrl in paginatedFeedUrls)
                    {
                        var feedDetails = new RssFeed { Link = feedUrl };

                        var favoriteFeed = favoriteFeeds.FirstOrDefault(f => f.Link == feedDetails.Link);
                        if (favoriteFeed != null)
                        {
                            feedDetails.IsFavorite = true;
                        }

                        feedsDetails.Add(feedDetails);
                    }

                    var totalPages = (int)Math.Ceiling((double)feedUrls.Count / PageSize);
                    var result = new { CurrentPage = page, TotalPages = totalPages, FeedsDetails = feedsDetails };

                    return Ok(result);
                }
                else
                {
                    _logger.LogError("Unsuccessful status code");
                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request.");
                return StatusCode(500);
            }
        }

        private List<string> ParseOpml(string opmlContent)
        {
            var feedUrls = new List<string>();

            XDocument opmlDoc = XDocument.Parse(opmlContent);
            var outlineElements = opmlDoc.Descendants("outline");

            foreach (var outlineElement in outlineElements)
            {
                var xmlUrlAttribute = outlineElement.Attribute("xmlUrl");
                if (xmlUrlAttribute != null)
                {
                    var feedUrl = xmlUrlAttribute.Value;
                    feedUrls.Add(feedUrl);
                }
            }

            return feedUrls;
        }

        // ...
    }

    public class RssFeed
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public bool IsFavorite { get; set; } = false;
    }
}
