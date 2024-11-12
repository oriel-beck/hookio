using Hookio.Contracts.Subscription;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController(ISubscriptionManager subscriptionManager) : ControllerBase
    {
        private readonly ISubscriptionManager _dataManager = subscriptionManager;
        
        [HttpPost("{guildId}")]
        public async Task<ActionResult<SubscriptionResponse?>> CreateSubscription(ulong guildId, SubscriptionRequest request, CancellationToken cancellationToken)
        {
            var result = await _dataManager.Create(guildId, request, cancellationToken);
            if (result == null)
            {
                return StatusCode(500, new GeneralError(500, "Unknown error, failed to create subscription"));
            }
            return Ok(result);
        }

        [HttpGet("{guildId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionResponse>>> GetSubscriptions(ulong guildId, [FromQuery] SubscriptionFilter filter, CancellationToken cancellationToken) =>
            Ok(await _dataManager.Get(guildId, filter, cancellationToken));

        [HttpGet("{guildId}/{subscriptionId:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> GetSubscription(ulong guildId, int subscriptionId, CancellationToken cancellationToken)
        {
            var result = await _dataManager.Get(guildId, subscriptionId, cancellationToken);
            if (result == null) return NotFound(new GeneralError(404, $"Cannot find subscription {subscriptionId} in guild {guildId}"));
            return Ok(result);
        }

        [HttpPatch("{guildId}/{subscriptionId:int}")]
        public async Task<ActionResult<SubscriptionResponse?>> PatchSubscription(ulong guildId, int subscriptionId, SubscriptionPatch patch, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _dataManager.Patch(guildId, subscriptionId, patch, cancellationToken);
                if (result == null) return NotFound(new GeneralError(404, $"Failed to patch subscription {subscriptionId}"));
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new GeneralError(500, $"Unknown error, failed to patch subscription {subscriptionId}"));
            }
        }
    }
}
