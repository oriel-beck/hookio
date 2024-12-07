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
        IYouTubeSubscriptionCache youTubeSubscriptionCache
        ) : ControllerBase
    {
        private readonly Shared.Options.YouTube _options = youtubeOptions.Value;
        private readonly IYouTubeSubscriptionCache _subscriptionCache = youTubeSubscriptionCache;

        [HttpGet("callback")]
        public async Task<IActionResult> Subscribe(
            [BindRequired, FromQuery(Name = "hub.mode")] string hubMode,
            [BindRequired, FromQuery(Name = "hub.topic")] string hubTopic,
            [BindRequired, FromQuery(Name = "hub.challenge")] string hubChallenge
            )
        {
            // Check that the hub.topic corresponds to a pending subscription or unsubscription
            var subscription = await _subscriptionCache.GetByTopicUrlAsync(hubTopic);
            if (subscription == null)
            {
                return NotFound("No matching subscription found.");
            }

            if (hubMode == "subscribe" || hubMode == "unsubscribe")
            {
                // Respond with the hub.challenge value
                return Content(hubChallenge, "text/plain");
            }

            return BadRequest("Invalid hub.mode.");
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Notify()
        {
            var valid = ValidateRequest(HttpContext.Request);
            if (!valid) return Unauthorized("Invalid Hub Signature");
            return Ok();
        }

        private bool ValidateRequest(HttpRequest request)
        {
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
                    return false;
                }
            }
            return false;
        }
    }
}
