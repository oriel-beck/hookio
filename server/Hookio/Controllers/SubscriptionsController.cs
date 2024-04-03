using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.Enunms;
using Hookio.ModelBindings;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("/api/subscriptions")]
    [ApiController]
    public class SubscriptionsController(ILogger<SubscriptionsController> logger, IDataManager dataManager) : ControllerBase
    {
        [HttpGet("{guildId}/{provider?}")]
        public async Task<ActionResult<List<SubscriptionResponse>>> GetGuildSubscription([DiscordGuildId] ulong guildId, SubscriptionType? provider)
        {
            if(!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            var announcements = await dataManager.GetSubscriptions(guildId, provider);
            logger.LogInformation($"[{nameof(GetGuildSubscription)}]: returned '{announcements?.Count}' announcements for '{guildId}'");
            return Ok(announcements);
        }

        [HttpPost("{guildId}")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription([DiscordGuildId] ulong guildId, SubscriptionRequest subscription)
        {
            if (!Util.CanAccessGuild(HttpContext.User, guildId)) return Unauthorized();
            // TODO: subscribe to selected provider
            var subscriptionResult = await dataManager.CreateSubscription(guildId, subscription);
            return Ok(subscriptionResult);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetGuildSubscription(int id)
        {
            var subscription = await dataManager.GetSubscriptionById(id);
            if (subscription is not null && !Util.CanAccessGuild(HttpContext.User, subscription.GuildId)) return Unauthorized();

            return subscription is null ? NotFound() : Ok(subscription);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<SubscriptionResponse>> UpdateSubscription(int id)
        {
            var subscription = await dataManager.GetSubscriptionById(id);
            if (subscription is not null && !Util.CanAccessGuild(HttpContext.User, subscription.GuildId)) return Unauthorized();

            return Ok(subscription);
        }
    }
}
