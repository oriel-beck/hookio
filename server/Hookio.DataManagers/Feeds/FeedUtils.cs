using Hookio.Contracts;
using Hookio.Enunms;
using System.Xml.Linq;

namespace Hookio.Feeds
{
    public static class FeedUtils
    {
        private static bool IsUrl(string str) =>
str.StartsWith("http") && Uri.TryCreate(str, UriKind.Absolute, out _);

        private static string GetName(XElement element)
        {
            // If the element is in the default namespace, just return the local name.
            if (element.Name.Namespace == element.GetDefaultNamespace())
                return element.Name.LocalName;

            // If the element has a namespace prefix, return the prefixed name.
            var prefix = element.GetPrefixOfNamespace(element.Name.Namespace);
            return string.IsNullOrEmpty(prefix) ? element.Name.LocalName : $"{prefix}:{element.Name.LocalName}";
        }

        public async static Task<(List<TemplateStringResponse>, XMLDetails)> Parse(string url, HttpClient httpClient)
        {
            var xmlDetails = new XMLDetails();
            var templateStrings = new Dictionary<string, TemplateStringResponse>();
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("Accept", "*/*");
                message.Headers.Add("User-Agent", "Hookio 1.0.0");
                var res = await httpClient.SendAsync(message);
                XDocument xmlDoc = await XDocument.LoadAsync(res.Content.ReadAsStream(), LoadOptions.None, CancellationToken.None);
                var root = xmlDoc.Root;
                if (root != null)
                {
                    ParseChild(root);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the RSS feed: {ex.Message}");
            }

            void ParseChild(XElement child, string prefix = "")
            {
                var name = string.IsNullOrEmpty(prefix) ? GetName(child) : $"{prefix}.{GetName(child)}";

                if (child == null || templateStrings.ContainsKey(name)) return;

                if (!string.IsNullOrEmpty(child.Value.Trim()) && child.HasElements == false)
                {
                    if (child.Name.LocalName == "id" || child.Name.LocalName == "guid")
                    {
                        xmlDetails.Id = child.Value;
                    }

                    if (child.Name.LocalName == "updated" && DateTime.TryParse(child.Value, out var updated))
                    {
                        xmlDetails.Updated = updated;
                    }

                    if ((child.Name.LocalName == "published" || child.Name.LocalName == "pubDate") && DateTime.TryParse(child.Value, out var published))
                    {
                        xmlDetails.Published = published;
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

                foreach (var childNode in child.Elements())
                {
                    if (templateStrings.ContainsKey(name)) continue;
                    ParseChild(childNode, name);
                }
            }

            return (templateStrings.Select(t => t.Value).ToList(), xmlDetails);
        }

        public async static Task<(List<TemplateStringResponse>, XMLDetails)> Parse(HttpResponseMessage res)
        {
            var xmlDetails = new XMLDetails();
            var templateStrings = new Dictionary<string, TemplateStringResponse>();
            try
            {
                XDocument xmlDoc = await XDocument.LoadAsync(res.Content.ReadAsStream(), LoadOptions.None, CancellationToken.None);
                var root = xmlDoc.Root;
                if (root != null)
                {
                    ParseChild(root);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the RSS feed: {ex.Message}");
            }

            void ParseChild(XElement child, string prefix = "")
            {
                var name = string.IsNullOrEmpty(prefix) ? GetName(child) : $"{prefix}.{GetName(child)}";

                if (child == null || templateStrings.ContainsKey(name)) return;

                if (!string.IsNullOrEmpty(child.Value.Trim()) && child.HasElements == false)
                {
                    if (child.Name.LocalName == "id" || child.Name.LocalName == "guid")
                    {
                        xmlDetails.Id = child.Value;
                    }

                    if (child.Name.LocalName == "updated" && DateTime.TryParse(child.Value, out var updated))
                    {
                        xmlDetails.Updated = updated;
                    }

                    if ((child.Name.LocalName == "published" || child.Name.LocalName == "pubDate") && DateTime.TryParse(child.Value, out var published))
                    {
                        xmlDetails.Published = published;
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

                foreach (var childNode in child.Elements())
                {
                    if (templateStrings.ContainsKey(name)) continue;
                    ParseChild(childNode, name);
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
