using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace XmlFeedReader.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<XmlItem> Items { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGet()
        {
            var client = _httpClientFactory.CreateClient();
            string xmlUrl = "http://scripting.com/rss.xml";
            var response = await client.GetAsync(xmlUrl);

            if (response.IsSuccessStatusCode)
            {
                var xmlContent = await response.Content.ReadAsStringAsync();

                // Parse the XML content
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                // Get the root element
                var rootElement = xmlDoc.DocumentElement;

                // Create a list to hold the parsed items
                var items = new List<XmlItem>();

                // Iterate over the "item" elements
                var itemNodes = rootElement.SelectNodes("channel/item");
                foreach (XmlNode itemNode in itemNodes)
                {
                    var item = new XmlItem();
                    item.Title = itemNode.SelectSingleNode("title")?.InnerText;
                    item.Description = itemNode.SelectSingleNode("description")?.InnerText;
                    item.PubDate = itemNode.SelectSingleNode("pubDate")?.InnerText;
                    item.Link = itemNode.SelectSingleNode("link")?.InnerText;
                    item.Guid = itemNode.SelectSingleNode("guid")?.InnerText;

                    items.Add(item);
                }

                // Populate the Items property with the parsed data
                Items = items;
            }
            return Page();

        }

    }

    public class XmlItem
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PubDate { get; set; }
        public string? Link { get; set; }
        public string? Guid { get; set; }
    }

}