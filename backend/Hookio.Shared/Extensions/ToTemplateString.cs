namespace Hookio.Shared.Extensions
{
    public static class StringExtension
    {
        public static string ToTemplateString(this string template, Dictionary<string, string> templateData)
        {
            return TemplateEngine.RenderTemplate(template, templateData);
        }
    }
}
