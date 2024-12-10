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

        /// <summary>
        /// PubSubHubBub hub sends a validation callback back when you create a subscription, this handles it
        /// </summary>
        /// <param name="hubMode"></param>
        /// <param name="hubTopic"></param>
        /// <param name="hubChallenge"></param>
        /// <param name="hubVerifyToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// PubSubHubBub sends a notificatio as POST, this handles and decrypts it
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
            
            // We do not cache a video for more than 7d, so if the video is older simply ignore it
            if (videoData.Entry.Published < DateTime.UtcNow.AddDays(-7)) return Ok();
            var ytSubscription = _subscriptionCache.Get(videoData.Entry.ChannelId);
            ytSubscription ??= new()
            {
                Status = Shared.Enums.YouTubeSubscriptionStatus.Active,
                ChannelId = videoData.Entry.ChannelId
            };

            var channel = await _youtubeManager.GetYouTubeChannelDetails(ytSubscription, cancellationToken);
            var video = await _youtubeManager.GetYouTubeVideoDetails(ytSubscription, videoData.Entry.VideoId, cancellationToken);

            if (channel == null || video == null) return NotFound("Failed to find channel or video");
            
            // TODO: implement webhook parsing and sending + template strings

            return Ok();
        }
    }
}
