using Hookio.Contracts.Subscription;
using Hookio.Data;
using Hookio.Data.Entities;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Enums;
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
            Webhook? webhook = await ctx.Webhooks.FirstOrDefaultAsync(x => x.Id == webhookId, cancellationToken);

            if (webhook == null)
            {
                await ctx.AddAsync(webhook = new()
                {
                    Id = webhookId,
                    Token = webhookToken,
                }, cancellationToken);
            }

            Subscription res;
            await ctx.AddAsync(res = new()
            {
                GuildId = guildId,
                SubscriptionType = request.SubscriptionType,
                WebhookId = webhookId,
                Webhook = webhook,
            }, cancellationToken);

            // generate Id for subscription
            await ctx.SaveChangesAsync(cancellationToken);

            // generate initial messages
            res.Messages = GenerateInitialMessages(res);

            // save everything
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
            await ctx.Database.BeginTransactionAsync(cancellationToken);

            var res = await ctx.Subscriptions
                .Include(x => x.Messages)
                .Include(x => x.Webhook!)
                .ThenInclude(x => x.Subscriptions)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == subscriptionId, cancellationToken);

            if (res == null) return null;

            if (patch.WebhookUrl != null)
            {
                var webhookId = UpdateWebhook(res, patch.WebhookUrl, ctx, cancellationToken);
                res.WebhookId = webhookId;
            }

            if (patch.Messages != null)
            {
                if (patch.Messages.Any(m => !m.IsValid))
                {
                    await ctx.Database.RollbackTransactionAsync(cancellationToken);
                    // TODO: better exceptions (which message failed) and logging
                    throw new ValidationException("Message is invalid");
                }
                Parallel.ForEach(patch.Messages, msg =>
                {
                    var origin = res.Messages?.FirstOrDefault(m => m.Id == msg.Id);
                    if (origin == null) return;
                    origin.Content = msg.Content;
                    origin.Embeds = msg.Embeds;

                    // this can set things that if set manually can do stupid shit, but they can't break the app.
                    // If a user decides to send a custom value here to break their shit, they only break their shit.
                    origin.Type = msg.Type;
                    origin.Action = msg.Action;
                });
            }

            if (patch.Source != null)
            {
                // TODO: implement
            }

            await ctx.Database.CommitTransactionAsync(cancellationToken);
            await ctx.SaveChangesAsync(cancellationToken);

            return Contractor.ToContract(res);
        }

        private static List<Message> GenerateInitialMessages(Subscription subscription)
        {
            return subscription.SubscriptionType switch
            {
                SubscriptionType.YouTube => new()
                    {
                        { new() { Action = null, Type = MessageType.YouTubeVideoCreated, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.YouTubeVideoUpdated, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.YouTubeVideoDeleted, SubscriptionId = subscription.Id } }
                    },
                SubscriptionType.Twitch => new()
                    {
                        { new() { Action = null, Type = MessageType.TwitchStreamStarted, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.TwitchStreamUpdated, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.TwitchStreamEnded, SubscriptionId = subscription.Id } }
                    },
                _ => [],
            };
        }

        private static ulong UpdateWebhook(Subscription subscription, string webhookUrl, HookioContext ctx, CancellationToken cancellationToken)
        {
            var validWebhook = TestAndParseWebhookUrl(webhookUrl, out ulong webhookId, out string webhookToken);
            if (!validWebhook) throw new ValidationException("Invalid webhook URL");
            var webhook = subscription.Webhook!;

            // if the new webhook is not the original webhook, check if the original webhook has more than 1 subscriptions, and if not, remove it
            if (webhook.Id != webhookId)
            {
                if (webhook.Subscriptions.Count() == 1) ctx.Remove(webhook);
                ctx.Webhooks.Add(webhook = new() 
                { 
                    Id = webhookId ,
                    Token = webhookToken
                });
            }
            return webhook.Id;
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
