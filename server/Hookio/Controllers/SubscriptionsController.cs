using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.Enums;
using Hookio.Exceptions;
using Hookio.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Controllers
{
    [Authorize]
    [Route("api/subscriptions")]
    [ApiController]
    public class SubscriptionsController(ILogger<SubscriptionsController> logger, ISubscriptionService subscriptions) : ControllerBase
    {
        [HttpGet("{guildId}")]
        public async Task<ActionResult<GuildSubscriptionsResponse>> GetGuildSubscriptions([DiscordGuildId] ulong guildId, SubscriptionType? subscriptionType, bool withCounts = false)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            var res = await subscriptions.GetSubscriptions(guildId, subscriptionType, withCounts);
            logger.LogInformation("[{FunctionName}]: returned '{SubscriptionsCount}' announcements{Addition} for guild, they have '{GlobalCount}' subscriptions", nameof(GetGuildSubscriptions), res?.Subscriptions.Count, (subscriptionType is not null ? $" of subscriptionType '{subscriptionType}'" : ""), res?.Count);
            return Ok(res);
        }

        [HttpPost("{guildId}")]
        [EnableRateLimiting("subscriptions")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription([DiscordGuildId] ulong guildId, SubscriptionRequest subscription)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            try
            {
                var subscriptionResult = await subscriptions.CreateSubscription(guildId, subscription);
                if (subscriptionResult == null) return NotFound();
                return CreatedAtAction(nameof(GetGuildSubscription), new { guildId, id = subscriptionResult.Id }, subscriptionResult);
            }
            catch (Exception ex) when (IsClientError(ex))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create subscription");
                return StatusCode(500, new { message = "Internal error, please try again later..." });
            }
        }

        [HttpGet("{guildId}/{id:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetGuildSubscription([DiscordGuildId] ulong guildId, int id)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            var subscription = await subscriptions.GetSubscription(guildId, id);

            return subscription is null ? NotFound() : Ok(subscription);
        }

        [HttpPatch("{guildId}/{id:int}")]
        [EnableRateLimiting("subscriptions")]
        public async Task<ActionResult<SubscriptionResponse>> UpdateSubscription([DiscordGuildId] ulong guildId, int id, SubscriptionRequest subscriptionRequest)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            try
            {
                var subscription = await subscriptions.UpdateSubscription(guildId, id, subscriptionRequest);
                if (subscription is null) return NotFound();
                return Ok(subscription);
            }
            catch (Exception ex) when (IsClientError(ex))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update subscription {Id}", id);
                return StatusCode(500, new { message = "Internal error, please try again later..." });
            }
        }

        [HttpDelete("{guildId}/{id:int}")]
        [EnableRateLimiting("subscriptions")]
        public async Task<IActionResult> DeleteSubscription([DiscordGuildId] ulong guildId, int id)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized("You cannot access this guild's subscriptions");
            var deleted = await subscriptions.DeleteSubscription(guildId, id);
            return deleted ? NoContent() : NotFound();
        }

        private static bool IsClientError(Exception ex) =>
            ex is EmbedTooLongException
            || ex is RequiresPremiumException
            || ex is InvalidChannelURLException
            || ex is FailedToSubscribeException
            || ex is ValidationException;
    }
}
