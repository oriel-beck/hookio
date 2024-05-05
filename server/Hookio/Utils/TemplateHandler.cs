﻿using Google.Apis.YouTube.v3.Data;
using System.Text.RegularExpressions;

namespace Hookio.Utils
{
    public partial class TemplateHandler(Dictionary<string, Dictionary<string, string>> data)
    {
        private readonly Dictionary<string, Dictionary<string, string>> data = data;

        public string? Parse(string? template)
        {
            if (template == null) return null;
            string result = template;

            // Find all matches of placeholders in the template
            MatchCollection matches = TemplateString().Matches(template);

            // Iterate over matches and replace placeholders
            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                string property = match.Groups[2].Value;

                if (data.TryGetValue(key, out var innerData))
                {
                    if (innerData.TryGetValue(property, out var value))
                    {
                        result = result.Replace(match.Value, value);
                    }
                    else if (property == "" && innerData.TryGetValue("default", out var defaultValue))
                    {
                        result = result.Replace(match.Value, defaultValue);
                    }
                }
            }

            return result;
        }
        
        public static TemplateHandler InitiateYoutubeTemplateHandler(Video video, Channel channel)
        {
            return new TemplateHandler(new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "channel", new()
                    {
                        { "default", video.Snippet.ChannelTitle },
                        { "name", video.Snippet.ChannelTitle },
                        { "url", channel.Snippet.CustomUrl },
                        { "description", channel.Snippet.Description },
                        { "icon", channel.Snippet.Thumbnails.Standard.Url },
                        { "subscribersCount", channel.Statistics.SubscriberCount.ToString()! },
                        { "viewsCount", channel.Statistics.VideoCount.ToString()! },
                        { "videosCount", channel.Statistics.VideoCount.ToString()! },
                        { "icon", channel.Snippet.Thumbnails.Default__.Url }
                    }
                },
                {
                    "video", new()
                    {
                        { "default", $"https://www.youtube.com/watch?v={video.Id}" },
                        { "title", video.Snippet.Title },
                        { "description", video.Snippet.Description },
                        { "url", $"https://www.youtube.com/watch?v={video.Id}" },
                        { "thumbnail", video.Snippet.Thumbnails.Standard.Url },
                        // TODO: parse this to human readable time
                        { "duration", video.ContentDetails.Duration },
                    }
                }
            });
        }

        [GeneratedRegex(@"\{([A-Za-z]+)(?:\(([A-Za-z]+)\))?\}")]
        private static partial Regex TemplateString();
    }

}