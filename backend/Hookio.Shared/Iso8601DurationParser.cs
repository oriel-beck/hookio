using System.Text;

namespace Hookio.Shared
{
    /// <summary>
    /// This class converts ISO8601 duration to readable time.
    /// Examples:
    /// PT41M15S - 42 minutes and 21 seconds
    /// PT3H22M44S - 3 hours, 22 minutes and 44 seconds
    /// P0D (yt stream) - 0 seconds (maybe should put something else here)
    /// </summary>
    public static class Iso8601DurationParser
    {
        public static string ParseToReadableTime(string iso8601Duration)
        {
            if (string.IsNullOrEmpty(iso8601Duration))
                throw new ArgumentException("ISO 8601 duration cannot be null or empty.", nameof(iso8601Duration));

            if (!iso8601Duration.StartsWith("P"))
                throw new ArgumentException("Invalid ISO 8601 duration format.", nameof(iso8601Duration));

            // Remove the leading "P" and split into date and time components
            iso8601Duration = iso8601Duration[1..];
            string[] parts = iso8601Duration.Split('T');
            string datePart = parts.Length > 0 ? parts[0] : string.Empty;
            string timePart = parts.Length > 1 ? parts[1] : string.Empty;

            // Initialize components
            int years = 0, months = 0, days = 0, hours = 0, minutes = 0, seconds = 0;

            // Parse date component
            if (!string.IsNullOrEmpty(datePart))
            {
                years = ExtractComponent(datePart, 'Y');
                months = ExtractComponent(datePart, 'M');
                days = ExtractComponent(datePart, 'D');
            }

            // Parse time component
            if (!string.IsNullOrEmpty(timePart))
            {
                hours = ExtractComponent(timePart, 'H');
                minutes = ExtractComponent(timePart, 'M');
                seconds = ExtractComponent(timePart, 'S');
            }

            // Build the readable format
            var builder = new StringBuilder();

            if (years > 0) builder.Append($"{years} year{(years > 1 ? "s" : "")}");
            if (months > 0) AppendWithAnd(builder, $"{months} month{(months > 1 ? "s" : "")}");
            if (days > 0) AppendWithAnd(builder, $"{days} day{(days > 1 ? "s" : "")}");
            if (hours > 0) AppendWithAnd(builder, $"{hours} hour{(hours > 1 ? "s" : "")}");
            if (minutes > 0) AppendWithAnd(builder, $"{minutes} minute{(minutes > 1 ? "s" : "")}");
            if (seconds > 0) AppendWithAnd(builder, $"{seconds} second{(seconds > 1 ? "s" : "")}");

            return builder.Length > 0 ? builder.ToString() : "0 seconds";
        }

        private static int ExtractComponent(string part, char designator)
        {
            int index = part.IndexOf(designator);
            if (index == -1) return 0;

            int start = index - 1;
            while (start >= 0 && char.IsDigit(part[start]))
            {
                start--;
            }
            start++;

            string value = part[start..index];
            return int.TryParse(value, out int result) ? result : 0;
        }

        private static void AppendWithAnd(StringBuilder builder, string value)
        {
            if (builder.Length > 0)
            {
                if (builder.ToString().Contains(" and "))
                    builder.Append(", ");
                else
                    builder.Append(" and ");
            }
            builder.Append(value);
        }
    }
}
