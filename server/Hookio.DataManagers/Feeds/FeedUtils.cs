using Hookio.Contracts;
using Hookio.Enums;
using System.Xml.Linq;

namespace Hookio.Feeds
{
    public static class FeedUtils
    {
        private static bool IsUrl(string str) =>
            str.StartsWith("http") && Uri.TryCreate(str, UriKind.Absolute, out _);

        private static string GetName(XElement element)
        {
            if (element.Name.Namespace == element.GetDefaultNamespace())
                return element.Name.LocalName;

            var prefix = element.GetPrefixOfNamespace(element.Name.Namespace);
            return string.IsNullOrEmpty(prefix) ? element.Name.LocalName : $"{prefix}:{element.Name.LocalName}";
        }

        public static async Task<(List<TemplateStringResponse>, XMLDetails)> Parse(string url, HttpClient httpClient)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Add("Accept", "*/*");
            message.Headers.Add("User-Agent", "Hookio 1.0.0");
            var res = await httpClient.SendAsync(message);
            return await Parse(res);
        }

        public static async Task<(List<TemplateStringResponse>, XMLDetails)> Parse(HttpResponseMessage res)
        {
            try
            {
                XDocument xmlDoc = await XDocument.LoadAsync(res.Content.ReadAsStream(), LoadOptions.None, CancellationToken.None);
                return Parse(xmlDoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the RSS feed: {ex.Message}");
                return ([], new XMLDetails());
            }
        }

        public static (List<TemplateStringResponse>, XMLDetails) Parse(XDocument xmlDoc)
        {
            var xmlDetails = new XMLDetails();
            var templateStrings = new Dictionary<string, TemplateStringResponse>();
            var root = xmlDoc.Root;
            if (root != null)
            {
                ParseChild(root);
            }

            void ParseChild(XElement child, string prefix = "", bool insideLatestItem = false, bool skippedSiblingItems = false)
            {
                var name = string.IsNullOrEmpty(prefix) ? GetName(child) : $"{prefix}.{GetName(child)}";
                if (child == null) return;

                var isItem = child.Name.LocalName is "entry" or "item";
                if (isItem)
                {
                    if (skippedSiblingItems) return;
                    insideLatestItem = true;
                }

                if (!string.IsNullOrEmpty(child.Value.Trim()) && child.HasElements == false)
                {
                    if (insideLatestItem)
                    {
                        if (child.Name.LocalName is "id" or "guid")
                        {
                            xmlDetails.Id = child.Value;
                        }

                        if (child.Name.LocalName == "updated" && DateTime.TryParse(child.Value, out var updated))
                        {
                            xmlDetails.Updated = updated.ToUniversalTime();
                        }

                        if ((child.Name.LocalName is "published" or "pubDate") && DateTime.TryParse(child.Value, out var published))
                        {
                            xmlDetails.Published = published.ToUniversalTime();
                        }
                    }

                    templateStrings.TryAdd(name, new TemplateStringResponse
                    {
                        Key = name,
                        Value = child.Value.Trim(),
                        Type = IsUrl(child.Value.Trim()) ? TemplateStringType.Url : TemplateStringType.String
                    });
                }

                foreach (var attr in child.Attributes())
                {
                    var attrName = $"{name}#{attr.Name.LocalName}";
                    templateStrings.TryAdd(attrName, new TemplateStringResponse
                    {
                        Key = attrName,
                        Value = attr.Value,
                        Type = IsUrl(attr.Value) ? TemplateStringType.Url : TemplateStringType.String
                    });
                }

                var tookItem = false;
                foreach (var childNode in child.Elements())
                {
                    var childIsItem = childNode.Name.LocalName is "entry" or "item";
                    if (childIsItem)
                    {
                        if (tookItem) continue;
                        tookItem = true;
                        ParseChild(childNode, name, true, false);
                        continue;
                    }
                    ParseChild(childNode, name, insideLatestItem, skippedSiblingItems);
                }
            }

            return (templateStrings.Select(t => t.Value).ToList(), xmlDetails);
        }
    }

    public class XMLDetails
    {
        public DateTime? Updated { get; set; }
        public DateTime? Published { get; set; }
        public string? Id { get; set; }
    }
}
