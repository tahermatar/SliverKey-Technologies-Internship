using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FavouriteFeeds.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;
        private const string FavouritesCookieName = "Favourites";
        public int PageSize { get; set; } = 10;
        public List<RssFeed> FeedsDetails { get; set; } = new List<RssFeed>();

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet([FromQuery] int page = 1)
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

                    foreach (var feedUrl in paginatedFeedUrls)
                    {
                        var feedDetails = new RssFeed { Link = feedUrl };

                        var favoriteFeed = favoriteFeeds.FirstOrDefault(f => f.Link == feedDetails.Link);
                        if (favoriteFeed != null)
                        {
                            feedDetails.IsFavorite = true;
                        }

                        FeedsDetails.Add(feedDetails);
                    }

                    ViewData["CurrentPage"] = page;
                    ViewData["TotalPages"] = (int)Math.Ceiling((double)feedUrls.Count / PageSize);

                    return Page();
                }
                else
                {
                    _logger.LogError("Unsuccessful status code");
                    return RedirectToPage("/Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request.");
                return RedirectToPage("/Error");
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
    }
}
public class RssFeed
{
    public string Title { get; set; }
    public string Link { get; set; }
    public string Description { get; set; }
    public string PubDate { get; set; }
    public bool IsFavorite { get; set; } = false;
}
