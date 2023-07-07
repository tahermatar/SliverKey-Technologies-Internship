using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace DealingWithOPML.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<RssItem> PaginatedRssItems { get; set; }
        public int PageSize { get; private set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGet(int pageNumber = 1)
        {
            var client = _httpClientFactory.CreateClient();

            // Fetch the OPML file
            string opmlUrl = "https://blue.feedland.org/opml?screenname=dave";
            var opmlResponse = await client.GetAsync(opmlUrl);

            if (opmlResponse.IsSuccessStatusCode)
            {
                var opmlContent = await opmlResponse.Content.ReadAsStringAsync();

                // Parse the OPML file to extract RSS feed URLs
                var rssUrls = ParseOpml(opmlContent);

                // Fetch and parse the RSS feeds
                var rssItems = new List<RssItem>();
                foreach (var rssUrl in rssUrls)
                {
                    var rssResponse = await client.GetAsync(rssUrl);
                    if (rssResponse.IsSuccessStatusCode)
                    {
                        var rssContent = await rssResponse.Content.ReadAsStringAsync();
                        var items = ParseRss(rssContent);
                        rssItems.AddRange(items);
                    }
                }

                // Apply pagination
                const int pageSize = 10;
                CurrentPage = pageNumber;
                TotalPages = (int)Math.Ceiling((double)rssItems.Count / pageSize);
                PaginatedRssItems = rssItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }

            return Page();
        }

        private List<string> ParseOpml(string opmlContent)
        {
            var urls = new List<string>();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(opmlContent);

            var outlineNodes = xmlDoc.DocumentElement.SelectNodes("//outline[@xmlUrl]");
            foreach (XmlNode outlineNode in outlineNodes)
            {
                string url = outlineNode.Attributes["xmlUrl"].Value;
                urls.Add(url);
            }

            return urls;
        }

        private List<RssItem> ParseRss(string rssContent)
        {
            var items = new List<RssItem>();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(rssContent);

            var itemNodes = xmlDoc.DocumentElement.SelectNodes("//item");
            foreach (XmlNode itemNode in itemNodes)
            {
                var item = new RssItem();
                item.Title = itemNode.SelectSingleNode("title")?.InnerText;
                item.Description = itemNode.SelectSingleNode("description")?.InnerText;
                item.PubDate = itemNode.SelectSingleNode("pubDate")?.InnerText;
                item.Link = itemNode.SelectSingleNode("link")?.InnerText;

                items.Add(item);
            }

            return items;
        }
    }


    public class RssItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
    }
}
