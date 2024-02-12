using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.Enunms;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("/api/subscriptions")]
    [ApiController]
    public class SubscriptionsController(ILogger<SubscriptionsController> logger, IDataManager dataManager) : ControllerBase
    {
        // TODO: custom guildId param validator
        [HttpGet("{guildId}/{provider?}")]
        public async Task<ActionResult<List<SubscriptionResponse>>> GetGuildSubscription(string guildId, SubscriptionType? provider)
        {
            // TODO: validate user can access this guildId
            var converted = ulong.TryParse(guildId, out var result);
            if (!converted || guildId.Length < 17 || guildId.Length > 19) return BadRequest(new
            {
                Error = $"'{guildId}' is not a valid guild ID"
            });

            var announcements = await dataManager.GetSubscriptions(result, provider);
            logger.LogInformation($"[{nameof(GetGuildSubscription)}]: returned '{announcements?.Count}' announcements for '{guildId}'");
            return Ok(announcements);
        }

        [HttpPost("{guildId}")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription(string guildId)
        {
            var converted = ulong.TryParse(guildId, out var result);
            if (!converted || guildId.Length < 17 || guildId.Length > 19) return BadRequest(new
            {
                Error = $"{guildId} is not a valid guild ID"
            });

            // TODO: subscribe to youtube provider, and create an announcement in the database, and validate the user can access this guildId
            return Ok(true);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetGuildSubscription(int id)
        {
            // TODO: verify that the user can access the server that owns this subscription
            var announcement = await dataManager.GetSubscriptionById(id);
            return Ok(announcement);
        }
    }
}
