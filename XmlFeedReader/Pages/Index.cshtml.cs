using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace XmlFeedReader.Pages
{
    public class IndexModel : PageModel
    {
        public List<XmlItem> Items { get; set; } = new();

        public void OnGet()
        {
            // Fetch the XML data using HttpClient
            using (var client = new HttpClient())
            {
                string xmlUrl = "http://scripting.com/rss.xml";
                var response = client.GetAsync(xmlUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var xmlContent = response.Content.ReadAsStringAsync().Result;

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
            }
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