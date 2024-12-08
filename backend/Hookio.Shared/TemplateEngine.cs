using System.Text;

namespace Hookio.Shared
{
    public static class TemplateEngine
    {
        /// <summary>
        /// Renders a template string by replacing placeholders with actual data.
        /// </summary>
        /// <param name="template">The template string with placeholders.</param>
        /// <param name="data">A dictionary containing keys and their respective replacement values.</param>
        /// <returns>The rendered string.</returns>
        public static string RenderTemplate(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Template cannot be null or empty.", nameof(template));
            if (data == null || data.Count == 0)
                return template;

            var result = new StringBuilder(template.Length);

            for (int i = 0; i < template.Length;)
            {
                // Find the start of a placeholder
                int start = template.IndexOf('{', i);
                if (start == -1)
                {
                    // No more placeholders, append the rest of the template
                    result.Append(template, i, template.Length - i);
                    break;
                }

                // Append text before the placeholder
                result.Append(template, i, start - i);

                // Find the end of the placeholder
                int end = template.IndexOf('}', start);
                if (end == -1)
                {
                    // No closing brace, treat as literal text
                    result.Append(template, start, template.Length - start);
                    break;
                }

                // Extract the placeholder key
                string key = template.Substring(start + 1, end - start - 1);

                // Replace placeholder with value or keep as-is if key is missing
                if (data.TryGetValue(key, out var value))
                    result.Append(value);
                else
                    result.Append(template, start, end - start + 1); // Keep placeholder

                // Move past the current placeholder
                i = end + 1;
            }

            return result.ToString();
        }
    }
}
