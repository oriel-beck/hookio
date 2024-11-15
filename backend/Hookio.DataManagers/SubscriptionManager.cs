using Hookio.Contracts.Subscription;
using Hookio.Data;
using Hookio.Data.Entities;
using Hookio.DataManagers.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hookio.DataManagers
{
    public partial class SubscriptionManager(IDbContextFactory<HookioContext> contextFactory) : ISubscriptionManager
    {
        private readonly IDbContextFactory<HookioContext> _contextFactory = contextFactory;

        [GeneratedRegex(@"^https:\/\/(canary|ptb|www\.)?discord\.com\/api\/webhooks\/(?<webhookId>\d+)\/(?<webhookToken>[\w-]+)$")]
        private static partial Regex WebhookRegex();

        public async Task<SubscriptionResponse?> Create(ulong guildId, SubscriptionRequest request, CancellationToken cancellationToken)
        {
            var validWebhook = TestAndParseWebhookUrl(request.WebhookUrl, out var webhookId, out var webhookToken);
            if (!validWebhook) throw new ValidationException("Invalid webhook URL");

            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
            Webhook webhook;

            await ctx.AddAsync(webhook = new() 
            {
                Id = webhookId,
                Token = webhookToken,
            }, cancellationToken);

            Subscription res;
            await ctx.AddAsync(res = new()
            {
                GuildId = guildId,
                SubscriptionType = request.SubscriptionType,
                WebhookId = webhookId,
                Webhook = webhook
            }, cancellationToken);

            await ctx.SaveChangesAsync(cancellationToken);

            return Contractor.ToContract(res);
        }

        public async Task<IEnumerable<SubscriptionResponse?>> Get(ulong guildId, SubscriptionFilter request, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var res = await ctx.Subscriptions.Where(x => x.GuildId == guildId).ToListAsync(cancellationToken);

            return res.Select(Contractor.ToContract);
        }

        public async Task<SubscriptionResponse?> Get(ulong guildId, int subscriptionId, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var res = await ctx.Subscriptions.FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == subscriptionId, cancellationToken);

            return Contractor.ToContract(res);
        }

        public async Task<SubscriptionResponse?> Patch(ulong guildId, int subscriptionId, SubscriptionPatch patch, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var res = await ctx.Subscriptions
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == subscriptionId, cancellationToken);

            if (res == null) return null;

            if (patch.WebhookUrl != null)
            {
                var validWebhook = TestAndParseWebhookUrl(patch.WebhookUrl, out var webhookId, out var webhookUrl);
                if (!validWebhook) throw new ValidationException("Invalid webhook URL");
            }

            if (patch.Messages != null)
            {
                Parallel.ForEach(res.Messages ?? [], message =>
                {
                    // If message is not found in the patch it was deleted
                    var found = patch.Messages.FirstOrDefault(m => m.Id == message.Id);
                    if (found != null)
                    {
                        message.Embeds = found.Embeds;
                        message.Content = found.Content;
                    }
                    else
                    {
                        ctx.Messages.Remove(message);
                    }
                });

                Parallel.ForEach(patch.Messages, message =>
                {
                    // Id is optional, no Id == new message
                    if (message.Id == null)
                    {
                        ctx.Messages.Add(new()
                        {
                            SubscriptionId = res.Id,
                            Content = message.Content,
                            Embeds = message.Embeds,
                            Subscription = res
                        });
                    }
                });
            }

            if (patch.Source != null)
            {
                // TODO: implement
            }

            await ctx.SaveChangesAsync(cancellationToken);

            return Contractor.ToContract(res);
        }

        private static bool TestAndParseWebhookUrl(string url, out ulong webhookId, out string webhookToken)
        {
            // Initialize output parameters
            webhookId = 0;
            webhookToken = string.Empty;

            Match match = WebhookRegex().Match(url);
            if (!match.Success)
            {
                return false;
            }

            // Extract and return the webhook ID and token
            webhookId = ulong.Parse(match.Groups["webhookId"].Value);
            webhookToken = match.Groups["webhookToken"].Value;

            // Basic validation
            if (webhookId == 0 || string.IsNullOrEmpty(webhookToken))
            {
                return false;
            }

            return true;
        }
    }
}
