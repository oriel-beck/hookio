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
        public async Task<ActionResult<List<SubscriptionResponse>>> GetGuildSubscription([DiscordGuildId] string guildId, SubscriptionType? provider)
        {
            // TODO: validate user can access this guildId
            _ = ulong.TryParse(guildId, out var result);

            var announcements = await dataManager.GetSubscriptions(result, provider);
            logger.LogInformation($"[{nameof(GetGuildSubscription)}]: returned '{announcements?.Count}' announcements for '{guildId}'");
            return Ok(announcements);
        }

        [HttpPost("{guildId}")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription([DiscordGuildId] string guildId)
        {
            _ = ulong.TryParse(guildId, out var result);
            // TODO: validate user can access this guildId


            // TODO: subscribe to youtube provider, and create an announcement in the database, and validate the user can access this guildId
            return Ok(true);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetGuildSubscription(int id)
        {
            // TODO: validate user can access this guildId that owns this subscription
            var announcement = await dataManager.GetSubscriptionById(id);
            return Ok(announcement);
        }
    }
}
