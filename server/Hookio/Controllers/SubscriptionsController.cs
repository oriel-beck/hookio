using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.Enunms;
using Hookio.Exceptions;
using Hookio.Shared.ModelBindings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hookio.Controllers
{
    [Authorize]
    [Route("/api/[controller]")]
    [ApiController]
    [EnableRateLimiting("subscriptions")]
    public class SubscriptionsController(ILogger<SubscriptionsController> logger, IDataManager dataManager) : ControllerBase
    {
        /// <summary>
        /// Get a guild's subscription
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="subscriptionType"></param>
        /// <param name="withCounts"></param>
        /// <returns code="200">All of the guilds's subscriptions</returns>
        /// <returns code="401">You do not have access to this guild ID</returns>
        [HttpGet("{guildId}")]
        public async Task<ActionResult<GuildSubscriptionsResponse>> GetGuildSubscription([DiscordGuildId] ulong guildId, SubscriptionType? subscriptionType, bool withCounts = false)
        {
            if(!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            var res = await dataManager.GetSubscriptions(guildId, subscriptionType, withCounts);
            logger.LogInformation("[{FunctionName}]: returned '{SubscriptionsCount}' announcements{Addition} for '{GuildId}', they have '{GlobalCount}' subscriptions", nameof(GetGuildSubscription), res?.Subscriptions.Count, (subscriptionType is not null ? $" of subscriptionType '{subscriptionType}'" : ""), guildId, res?.Count);
            return Ok(res);
        }

        /// <summary>
        /// Create a subscription for a guild ID
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="subscription"></param>
        /// <returns code="201">The created subscription</returns>
        /// <returns code="400">Invalid embed, too many subscription or invalid YT/Twitch channel ID</returns>
        /// <returns code="401">You do not have access to this guild ID</returns>
        /// <returns code="404">Failed to create a subscription</returns>
        /// <returns code="500">Something went wrong</returns>
        [HttpPost("{guildId}")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription([DiscordGuildId] ulong guildId, SubscriptionRequest subscription)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            try
            {
                var subscriptionResult = await dataManager.CreateSubscription(guildId, subscription);
                if (subscriptionResult == null) return NotFound();
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

        /// <summary>
        /// Return a specific subscription for a specific guild ID
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="id"></param>
        /// <returns code="200">A guild's subscription</returns>
        /// <returns code="404">Failed to find a subscription</returns>
        [HttpGet("{guildId}/{id:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetGuildSubscription([DiscordGuildId] ulong guildId, int id)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            var subscription = await dataManager.GetSubscription(guildId, id);

            return subscription is null ? NotFound() : Ok(subscription);
        }

        /// <summary>
        /// Updates a subscription for a specific guild ID
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="id"></param>
        /// <param name="subscriptionRequest"></param>
        /// <returns code="200">The updated subscription</returns>
        /// <returns code="401">You do not have permissions to update this subscription</returns>
        /// <returns code="404">The Subscription does not exist</returns>
        [HttpPatch("{guildId}/{id:int}")]
        public async Task<ActionResult<SubscriptionResponse>> UpdateSubscription([DiscordGuildId] ulong guildId, int id, SubscriptionRequest subscriptionRequest)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            var subscription = await dataManager.UpdateSubscription(guildId, id, subscriptionRequest);
            if (subscription is null) return NotFound();
            return Ok(subscription);
        }
    }
}
