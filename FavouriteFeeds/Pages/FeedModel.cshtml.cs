using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace XMLParser.Pages
{
    public class FeedModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FeedModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<FeedItem> FeedData { get; set; } = new List<FeedItem>();

        public async Task<IActionResult> OnGetAsync(string xmlUrl)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync(xmlUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);

                var nodes = xmlDoc.SelectNodes("rss/channel/item");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        var item = new FeedItem();
                        item.Title = node.SelectSingleNode("title")?.InnerText;
                        item.Description = node.SelectSingleNode("description")?.InnerText;
                        item.PubDate = node.SelectSingleNode("pubDate")?.InnerText;
                        item.Link = node.SelectSingleNode("link")?.InnerText;
                        FeedData.Add(item);
                    }
                }
            }

            return Page();
        }
    }

    public class FeedItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
    }
}
