using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace FavouriteFeeds.Pages
{
    public class FeedModel : PageModel
    {
        private readonly ILogger<FeedModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public FeedModel(ILogger<FeedModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public List<FeedItem> FeedData { get; set; } = new List<FeedItem>();

        public async Task<IActionResult> OnGet(string link)
        {
            try
            {
                if (string.IsNullOrEmpty(link))
                {
                    _logger.LogError("Feed link is not found");
                    return RedirectToPage("/Error");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(link);
                if (response.IsSuccessStatusCode)
                {
                    var xmlContent = await response.Content.ReadAsStringAsync();
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlContent);

                    var itemNodes = xmlDoc.SelectNodes("rss/channel/item");
                    if (itemNodes != null)
                    {
                        foreach (XmlNode itemNode in itemNodes)
                        {
                            var item = new FeedItem();
                            item.Description = itemNode.SelectSingleNode("description")?.InnerText;
                            item.PubDate = itemNode.SelectSingleNode("pubDate")?.InnerText;
                            item.Link = itemNode.SelectSingleNode("link")?.InnerText;
                            item.Guide = itemNode.SelectSingleNode("guid")?.InnerText;
                            FeedData.Add(item);
                        }
                    }
                }
                else
                {
                    _logger.LogError("Unsuccessful status code");
                    return RedirectToPage("/Error");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request.");
                return RedirectToPage("/Error");
            }
        }
    }

    public class FeedItem
    {
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
        public string Guide { get; set; }
    }
}
