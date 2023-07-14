using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace FavouriteFeeds.Pages
{

    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string FavouritesCookieName = "Favourites";

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<string> FeedTitles { get; set; } = new List<string>();
        public List<string> XmlUrls { get; set; } = new List<string>();
        public List<string> HtmlUrls { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            var favouritesJson = HttpContext.Request.Cookies[FavouritesCookieName];
            var favourites = JsonConvert.DeserializeObject<List<Feed>>(favouritesJson ?? "[]") ?? new List<Feed>();

            var client = _httpClientFactory.CreateClient();
            var opmlUrl = "https://blue.feedland.org/opml?screenname=dave";

            var opmlResponse = await client.GetAsync(opmlUrl);
            if (opmlResponse.IsSuccessStatusCode)
            {
                var opmlContent = await opmlResponse.Content.ReadAsStringAsync();
                var feedUrls = ParseOpml(opmlContent); // Replace with your own logic to parse the feed URLs from the OPML content

                foreach (var url in feedUrls)
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var feedTitle = ParseFeedTitle(content); // Replace with your own logic to parse feed title
                        FeedTitles.Add(feedTitle);
                        XmlUrls.Add(url);
                        HtmlUrls.Add(GetHtmlUrlFromXmlUrl(url)); // Replace with your own logic to get HTML URL from XML URL

                        // Check if the current feed is in the favourites list
                        var isFavourite = favourites.Any(f => f.XmlUrl == url);
                        if (isFavourite)
                        {
                            var feed = favourites.FirstOrDefault(f => f.XmlUrl == url);
                            feed.IsStarred = true;
                        }
                    }
                }
            }

            return Page();
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


        private string ParseFeedTitle(string xmlContent)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            var titleNode = xmlDoc.SelectSingleNode("//title");
            if (titleNode != null)
            {
                var feedTitle = titleNode.InnerText;
                return feedTitle;
            }

            return "Feed Title";
        }


        private string GetHtmlUrlFromXmlUrl(string xmlUrl)
        {
            var htmlUrl = xmlUrl.Replace(".xml", ".html");

            return htmlUrl;
        }

    }
}
public class Feed
{
    public string XmlUrl { get; set; }
    public bool IsStarred { get; set; }
}
