using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.Enunms;
using Hookio.Exceptions;
using Hookio.ModelBindings;
using Hookio.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hookio.Controllers
{
    [Authorize]
    [Route("/api/subscriptions")]
    [ApiController]
    [EnableRateLimiting("subscriptions")]
    public class SubscriptionsController(ILogger<SubscriptionsController> logger, IDataManager dataManager, IYoutubeService youtubeService) : ControllerBase
    {
        [HttpGet("{guildId}")]
        public async Task<ActionResult<GuildSubscriptionsResponse>> GetGuildSubscription([DiscordGuildId] ulong guildId, SubscriptionType? subscriptionType, bool withCounts = false)
        {
            if(!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            var res = await dataManager.GetSubscriptions(guildId, subscriptionType, withCounts);
            logger.LogInformation("[{FunctionName}]: returned '{SubscriptionsCount}' announcements{Addition} for '{GuildId}', they have '{GlobalCount}' subscriptions", nameof(GetGuildSubscription), res?.Subscriptions.Count, (subscriptionType is not null ? $" of subscriptionType '{subscriptionType}'" : ""), guildId, res?.Count);
            return Ok(res);
        }

        [HttpPost("{guildId}")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription([DiscordGuildId] ulong guildId, SubscriptionRequest subscription)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            // TODO: subscribe to selected provider
            try
            {
                var subscriptionResult = await dataManager.CreateSubscription(guildId, subscription);
                return Ok(subscriptionResult);
            }
            catch (Exception ex)
            {
                if (ex is EmbedTooLongException || ex is RequiresPremiumException || ex is InvalidChannelURLException)
                {
                    return BadRequest(new
                    {
                        ex.Message
                    });
                }

                // fallback
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    Message = "Internal error, please try again later..."
                });
                return StatusCode(500);
            }
        }

        [HttpGet("{guildId}/{id:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetGuildSubscription([DiscordGuildId] ulong guildId, int id)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            var subscription = await dataManager.GetSubscription(guildId, id);

            return subscription is null ? NotFound() : Ok(subscription);
        }

        [HttpPatch("{guildId}/{id:int}")]
        public async Task<ActionResult<SubscriptionResponse>> UpdateSubscription([DiscordGuildId] ulong guildId, int id, SubscriptionRequest subscriptionRequest)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            var subscription = await dataManager.UpdateSubscription(guildId, id, subscriptionRequest);

            return Ok(subscription);
        }
    }
}
