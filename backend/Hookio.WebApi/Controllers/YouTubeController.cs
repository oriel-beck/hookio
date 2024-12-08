using Hookio.DataManagers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Hookio.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController(
        IOptions<Shared.Options.YouTube> youtubeOptions,
        IYouTubeSubscriptionCache youTubeSubscriptionCache,
        IYouTubeManager youTubeManager
        ) : ControllerBase
    {
        private readonly Shared.Options.YouTube _options = youtubeOptions.Value;
        private readonly IYouTubeSubscriptionCache _subscriptionCache = youTubeSubscriptionCache;
        private readonly IYouTubeManager _youtubeManager = youTubeManager;

        [HttpGet("callback")]
        public IActionResult Subscribe(
            [BindRequired, FromQuery(Name = "hub.mode")] string hubMode,
            [BindRequired, FromQuery(Name = "hub.topic")] string hubTopic,
            [BindRequired, FromQuery(Name = "hub.challenge")] string hubChallenge,
            [BindRequired, FromQuery(Name = "hub.verify_token")] string hubVerifyToken
            )
        {
            // Check that the hub.topic corresponds to a pending subscription or unsubscription
            var subscription = _subscriptionCache.Get(hubTopic);
            if (subscription == null) return NotFound("No matching subscription found.");

            if (subscription.Status != Shared.Enums.YouTubeSubscriptionStatus.Pending) return BadRequest("Cannot change status of a not pending subscription");

            if (hubVerifyToken != subscription.VerifyToken) return Unauthorized("Invalid verify token for this subscription");

            if (hubMode == "subscribe" || hubMode == "unsubscribe")
            {
                // update the subscription to active status
                subscription.VerifyToken = string.Empty;
                subscription.Status = Shared.Enums.YouTubeSubscriptionStatus.Active;
                _subscriptionCache.Update(subscription);

                // Respond with the hub.challenge value
                return Content(hubChallenge, "text/plain");
            }

            return BadRequest("Invalid hub.mode.");
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Notify( CancellationToken cancellationToken)
        {
            var request = HttpContext.Request;

            // Get the raw body of the request
            using var reader = new StreamReader(Request.Body);
            var requestBody = reader.ReadToEnd();

            if (request.Headers.TryGetValue("X-Hub-Signature", out var signatureHeader))
            {
                // Compute the HMAC SHA-1 hash of the request body using the secret
                using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_options.HubSecret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
                var computedSignature = "sha1=" + BitConverter.ToString(hash).Replace("-", "").ToLower();

                // Compare the computed signature with the received signature
                if (!string.Equals(computedSignature, signatureHeader, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized("Invalid hub signature");
                }
            }

            var videoData = _youtubeManager.ParseYouTubePayload(requestBody);
            var subscription = _subscriptionCache.Get(videoData.Entry.ChannelId);
            subscription ??= new()
            {
                Status = Shared.Enums.YouTubeSubscriptionStatus.Active,
                ChannelId = videoData.Entry.ChannelId
            };

            var channel = await _youtubeManager.GetYouTubeChannelDetails(subscription, cancellationToken);
            var video = await _youtubeManager.GetYouTubeVideoDetails(subscription, cancellationToken);

            if (channel == null || video == null) return NotFound("Failed to find channel or video");

            // TODO: implement webhook parsing and sending + template strings

            return Ok();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSubscription([FromQuery] string channelId, CancellationToken cancellationToken)
        {
            var creation = await _youtubeManager.Subscribe(channelId, cancellationToken);
            return Ok(creation?.StatusCode);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            var xml = "<feed xmlns:yt=\"http://www.youtube.com/xml/schemas/2015\" xmlns=\"http://www.w3.org/2005/Atom\">\r\n  <link rel=\"hub\" href=\"https://pubsubhubbub.appspot.com\" />\r\n  <link rel=\"self\" href=\"https://www.youtube.com/xml/feeds/videos.xml?channel_id=UCeBMccz-PDZf6OB4aV6a3eA\" />\r\n  <title>YouTube video feed</title>\r\n  <updated>2024-12-08T16:42:48.041792159+00:00</updated>\r\n  <entry>\r\n    <id>yt:video:gBW9PEAnjqM</id>\r\n    <yt:videoId>gBW9PEAnjqM</yt:videoId>\r\n    <yt:channelId>UCeBMccz-PDZf6OB4aV6a3eA</yt:channelId>\r\n    <title>MINIONS SSF !Build | !Nord !IRL | First Day In Northern Ireland! https://youtu.be/d3V0qkMCyHI</title>\r\n    <link rel=\"alternate\" href=\"https://www.youtube.com/watch?v=gBW9PEAnjqM\" />\r\n    <author>\r\n      <name>Kripparrian</name>\r\n      <uri>https://www.youtube.com/channel/UCeBMccz-PDZf6OB4aV6a3eA</uri>\r\n    </author>\r\n    <published>2024-12-08T16:40:08+00:00</published>\r\n    <updated>2024-12-08T16:42:48.041792159+00:00</updated>\r\n  </entry>\r\n</feed>\r\n";
            var res = _youtubeManager.ParseYouTubePayload(xml);
            return Ok(res);
        }
    }
}
