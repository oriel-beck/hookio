using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Hookio.Database.Interfaces;
using Hookio.Utils;
using Hookio.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;

namespace Hookio.Controllers
{
    [Route("/api/youtube")]
    [ApiController]
    public class YoutubeController(ILogger<YoutubeController> logger, IYoutubeService youtubeService, IDataManager dataManager) : ControllerBase
    {
        private readonly YouTubeService ytService = new(new BaseClientService.Initializer()
        {
            ApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY"),
            ApplicationName = "yt-announcements"
        });

        [HttpGet("callback")]
        [HttpPost("callback")]
        public async Task<ActionResult> NotificationsCallback()
        {
            var request = HttpContext.Request;
            // TODO: use hub.secret when creating the subscriptions to verify source

            //var stringBuilder = new StringBuilder();
            //foreach (var item in request.Query) 
            //{
            //    stringBuilder.Append($"{item.Key}: {item.Value.FirstOrDefault()}\n");
            //}
            //logger.LogInformation("Incoming yt callback query: {Query}", stringBuilder.ToString());

            var challengeResponse = request.Query["hub.challenge"].FirstOrDefault();
            if (string.IsNullOrEmpty(challengeResponse))
            {
                var stream = request.Body;
                var data = await youtubeService.ConvertFromXml(stream);
                if (data == null || data.VideoId == null)
                {
                    logger.LogInformation("malformed data was recieved, aborting");
                    return Ok();
                }
                
                // TODO: check video publish, if video was published more than 1d ago, skip it.
                var videoListRequest = ytService.Videos.List("snippet, contentDetails");
                videoListRequest.Id = data.VideoId;
                var videoList = await videoListRequest.ExecuteAsync();
                var video = videoList.Items.FirstOrDefault();

                if (video != null)
                {
                    var channelRequest = ytService.Channels.List("snippet");
                    channelRequest.Id = video.Snippet.ChannelId;
                    var channelList = await channelRequest.ExecuteAsync();
                    var channel = channelList.Items.FirstOrDefault();

                    if (channel != null)
                    {
                        var exists = await youtubeService.IsNewVideo(data.VideoId);
                        if (exists == null)
                        {
                            logger.LogInformation("publishing new video for {Channel}, video: {VideoUrl}", channel.Snippet.CustomUrl, data.Link.Href);
                            youtubeService.PublishVideo(video, channel, dataManager);
                        }
                        else
                        {
                            logger.LogInformation("updating video for {Channel}, video: {VideoUrl}", channel.Snippet.CustomUrl, data.Link.Href);
                            youtubeService.UpdateVideo(video, channel, dataManager);
                        }
                    }

                }
                return Ok();
            }
            else
            {
                // this is a subscription confirmation
                logger.LogInformation("handling subscription confirmation with challenge {Challenge}", challengeResponse!);

                var topicUri = request.Query.TryGetValue("hub.topic", out var uriString);
                if (topicUri && Uri.TryCreate(uriString!, UriKind.Absolute, out Uri? uri))
                {
                    var queryParams = uri.Query
                        .TrimStart('?')
                        .Split('&')
                        .Select(param => param.Split('='))
                        .ToDictionary(parts => parts[0], parts => parts[1]);
                    queryParams.TryGetValue("channel_id", out var channelId);
                    request.Query.TryGetValue("hub.lease_seconds", out var leaseSecondsString);
                    _ = ulong.TryParse(leaseSecondsString, out var leaseSeconds);
                    await youtubeService.AddResub(channelId!, leaseSeconds);
                }
                var contentResult = new ContentResult
                {
                    Content = challengeResponse,
                    ContentType = "text/plain",
                    StatusCode = 200 // OK status code
                };
                return contentResult;
            }
        }
    }
}
