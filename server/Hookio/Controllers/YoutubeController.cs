﻿using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Hookio.Database.Interfaces;
using Hookio.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            var token = request.Query.TryGetValue("hub.verify_token", out var verifyToken);
            if (!token || !youtubeService.VerifyToken(verifyToken!))
            {
                return Unauthorized(new
                {
                    Error = "Invalid verify_token"
                });
            }

            var challenge = request.Query.TryGetValue("hub.challenge", out var challengeResponse);
            if (!challenge)
            {
                var stream = request.Body;
                var data = youtubeService.ConvertAtomToSyndication(stream);
                var videoListRequest = ytService.Videos.List("snippet, contentDetails");
                videoListRequest.Id = data.VideoId;
                var videoList = await videoListRequest.ExecuteAsync();
                var video = videoList.Items.FirstOrDefault();

                if (video != null)
                {
                    var channelRequest = ytService.Channels.List("snippet, statistics");
                    channelRequest.Id = video.Snippet.ChannelId;
                    var channelList = await channelRequest.ExecuteAsync();
                    var channel = channelList.Items.FirstOrDefault();

                    if (channel != null)
                    {
                        if (data.IsNewVideo)
                        {
                            youtubeService.PublishVideo(video, channel, dataManager);
                        }
                        else
                        {
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
                return Ok(new
                {
                    Content = new StringContent(challengeResponse!),
                    ReasonPhrase = challenge,
                    StatusCode = HttpStatusCode.OK
                });
            }
        }
    }
}
