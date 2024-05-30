using Hookio.Contracts;
using Hookio.DataManagers.Utils.Interfaces;
using System.Text.RegularExpressions;

namespace Hookio.Utils;
public partial class TemplateHandler(List<TemplateStringResponse> templateStrings) : ITemplateHandler
{
    private readonly Dictionary<string, string> _templateStrings = templateStrings.ToDictionary(item => item.Key, item => item.Value);

    public string? Parse(string? template)
    {
        if (template == null) return null;

        // Use the provided regular expression to find placeholders
        string result = TemplateRegex().Replace(template, ReplacePlaceholder);

        return result;
    }

    private string ReplacePlaceholder(Match match)
    {
        // The entire string inside {} is the key
        string key = match.Groups[1].Value;

        // Check if the key exists in the data dictionary
        if (_templateStrings.TryGetValue(key, out var value))
        {
            return value;
        }

        // Return the original placeholder if no replacement found
        return match.Value;
    }

    [GeneratedRegex(@"\{([^{}]*)\}")]
    private static partial Regex TemplateRegex();
}

