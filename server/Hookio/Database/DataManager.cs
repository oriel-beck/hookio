using Discord;
using Discord.Rest;
using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Hookio.Enunms;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database
{
    public class DataManager(IDbContextFactory<HookioContext> contextFactory) : IDataManager
    {
        private readonly HttpClient _http = new()
        {
            BaseAddress = new Uri("https://discord.com")
        };

        #region users
        public async Task<IEnumerable<RestUserGuild>> GetUserServers(DiscordRestClient client)
        {
            var servers = await client.GetGuildSummariesAsync().FlattenAsync();
            return servers;
        }

        public async Task<CurrentUserResponse?> GetUser(ulong userId) 
        {
            // TODO: change this to use the database for getting the user data, save user data on login (avatar, id, etc)
            var accessToken = await GetAccessToken(userId);
            var client = new DiscordRestClient();
            await client.LoginAsync(TokenType.Bearer, accessToken);
            return await ToContract(client);
        }

        public async Task<User?> CreateUser(DiscordRestClient client, OAuth2ExchangeResponse token)
        {
            var ctx = contextFactory.CreateDbContext();
            var newUser = new User
            {
                AccessToken = token.AccessToken,
                ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(token.ExpiresIn),
                Id = client.CurrentUser.Id,
                RefreshToken = token.RefreshToken,
            };
            var currentUser = ctx.Users.SingleOrDefault(u => u.Id == client.CurrentUser.Id);
            if (currentUser == null)
            {
                await ctx.Users.AddAsync(newUser);
            } 
            else 
            {
                currentUser.RefreshToken = newUser.RefreshToken;
                currentUser.AccessToken = newUser.AccessToken;
                currentUser.ExpireAt = newUser.ExpireAt;
            }
            await ctx.SaveChangesAsync();
            return newUser;

        }

        public async Task<string?> GetAccessToken(ulong userId)
        {
            var ctx = contextFactory.CreateDbContext();
            var currentUser = ctx.Users.SingleOrDefault(u => u.Id == userId);
            if (currentUser is null) return null;
            if (currentUser.ExpireAt <  DateTimeOffset.UtcNow)
            {
                // generate a new access token and update the db
                var discordResponse = await _http.PostAsync($"/api/v10/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")! },
                    { "refresh_token", currentUser.RefreshToken }
                }
                ));
                var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>();
                currentUser.ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(result!.ExpiresIn);
                currentUser.AccessToken = result.AccessToken;
                currentUser.RefreshToken = result.RefreshToken;
                await ctx.SaveChangesAsync();
                return result.AccessToken;
            }
            else
            {
                return currentUser.AccessToken;
            }
        }
        #endregion

        #region announcements
        public async Task<SubscriptionResponse?> GetSubscriptionById(int id)
        {
            var ctx = await contextFactory.CreateDbContextAsync();

            var subscription = await ctx.Subscriptions
                .Include(s => s.Messages)
                    .ThenInclude(m => m!.Embeds)
                        .ThenInclude(e => e.Fields)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null)
            {
                return null; // Subscription not found
            }

            return new SubscriptionResponse
            {
                Id = subscription.Id,
                AnnouncementType = subscription.SubscriptionType,
                ChannelId = subscription.ChannelId,
                Messages = subscription.Messages.Select(MapToMessageResponse).ToList(),
            };
        }

        private MessageResponse MapToMessageResponse(Message message)
        {
            return new MessageResponse
            {
                Content = message.Content,
                Embeds = message.Embeds.Select(MapToEmbedResponse).ToList(),
                Avatar = message.webhookAvatar,
                Username = message.webhookUsername,
            };
        }

        private EmbedResponse MapToEmbedResponse(Entities.Embed embed)
        {
            return new EmbedResponse
            {
                Id = embed.Id,
                Description = embed.Description,
                TitleUrl = embed.TitleUrl,
                Title = embed.Title,
                Color = embed.Color,
                Image = embed.Image,
                Author = embed.Author,
                AuthorUrl = embed.AuthorUrl,
                AuthorIcon = embed.AuthorIcon,
                Footer = embed.Footer,
                FooterIcon = embed.FooterIcon,
                Thumbnail = embed.Thumbnail,
                AddTimestamp = embed.AddTimestamp,
                Fields = embed.Fields.Select(MapToEmbedFieldResponse).ToList()
            };
        }

        private EmbedFieldResponse MapToEmbedFieldResponse(Entities.EmbedField field)
        {
            return new EmbedFieldResponse
            {
                Id = field.Id,
                Name = field.Name,
                Value = field.Value,
                Inline = field.Inline
            };
        }

        // TODO: Add avatar and maybe buttons at some point
        public async Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request)
        {
            using var context = await contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            int length = 0;
            try
            {
                var subscription = new Subscription
                {
                    GuildId = guildId,
                    WebhookUrl = request.WebhookUrl,
                    ChannelId = request.ChannelId,
                    SubscriptionType = request.SubscriptionType,
                };
                context.Subscriptions.Add(subscription);

                var message = new Message
                {
                    Content = request.Message.Content,
                    Subscription = subscription,
                    webhookUsername = request.Message.Username,
                    webhookAvatar = request.Message.Avatar,
                };
                context.Messages.Add(message);
                length += request.Message.Content.Length;

                await context.SaveChangesAsync(); // SaveChangesAsync to generate SubscriptionId

                foreach (var embedRequest in request.Message.Embeds)
                {
                    var embed = new Entities.Embed
                    {
                        Description = embedRequest.Description,
                        TitleUrl = embedRequest.TitleUrl,
                        Title = embedRequest.Title,
                        Color = embedRequest.Color,
                        Image = embedRequest.Image,
                        Author = embedRequest.Author,
                        AuthorUrl = embedRequest.AuthorUrl,
                        AuthorIcon = embedRequest.AuthorIcon,
                        Thumbnail = embedRequest.Thumbnail,
                        Footer = embedRequest.Footer,
                        FooterIcon = embedRequest.FooterIcon,
                        Message = message // Set Message navigation property
                    };
                    context.Embeds.Add(embed);
                    length += embedRequest.Length;

                    await context.SaveChangesAsync(); // SaveChangesAsync to generate EmbedId

                    foreach (var embedFieldRequest in embedRequest.Fields)
                    {
                        var embedField = new Entities.EmbedField
                        {
                            Name = embedFieldRequest.Name,
                            Value = embedFieldRequest.Value,
                            Inline = embedFieldRequest.Inline,
                            Embed = embed // Set Embed navigation property
                        };
                        context.EmbedFields.Add(embedField);
                        length += embedFieldRequest.Length;
                    }
                }

                if (length > 6000)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await context.SaveChangesAsync(); // SaveChangesAsync to generate EmbedFieldIds

                await transaction.CommitAsync();

                return new SubscriptionResponse
                {
                    Id = subscription.Id,
                    AnnouncementType = subscription.SubscriptionType,
                    ChannelId = subscription.ChannelId
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception for handling at a higher level
            }
        }

        public async Task<List<SubscriptionResponse>?> GetSubscriptions(ulong guildId, SubscriptionType? provider)
        {
            var ctx = await contextFactory.CreateDbContextAsync();
            IQueryable<Subscription> query = ctx.Subscriptions;
            if (provider is not null)
            {
                query = query.Where(subscription => (subscription.GuildId == guildId) && (subscription.SubscriptionType == provider));
            }
            else
            {
                query = query.Where(subscription => subscription.GuildId == guildId);
            }

            query = query.Include(x => x.Messages).ThenInclude(m => m.Embeds).ThenInclude(e => e.Fields);

            var subscriptions = (await query.ToListAsync()).Select(subscription => new SubscriptionResponse
            {
                Id = subscription.Id,
                AnnouncementType = subscription.SubscriptionType,
                ChannelId = subscription.ChannelId,
                GuildId = guildId,
                Messages = subscription.Messages.Select(ToContract).ToList()
            });

            return [.. subscriptions];
        }
        #endregion

        #region contracts
        private async Task<CurrentUserResponse?> ToContract(DiscordRestClient client)
        {
            var user = client.CurrentUser;
            user ??= await client.GetCurrentUserAsync();
            if (user == null) return null;
            var currentUser = new CurrentUserResponse()
            {
                Discriminator = user.Discriminator,
                Id = user.Id,
                Username = user.GlobalName is null ? user.Username : user.GlobalName,
                // TODO: implement premium in the DB
                Premium = 0,
                Avatar = user.GetAvatarUrl(),
                Guilds = (await GetUserServers(client)).Where(guild => guild.Permissions.ManageGuild).Select(guild => new GuildResponse()
                {
                    Id = guild.Id.ToString(),
                    Name = guild.Name,
                    Icon = guild.IconUrl is null ? "fallback" : guild.IconUrl,
                }).ToList()
            };
            return currentUser;
        }

        private MessageResponse ToContract(Message message)
        {
            return new MessageResponse
            {
                Content = message.Content,
                Username = message.webhookUsername,
                Avatar = message.webhookAvatar,
                Embeds = message.Embeds.Select(ToContract).ToList()
            };
        }

        private EmbedResponse ToContract(Entities.Embed embed)
        {
            return new EmbedResponse
            {
                Title = embed.Title,
                TitleUrl = embed.TitleUrl,
                Description = embed.Description,
                Footer = embed.Footer,
                FooterIcon = embed.FooterIcon,
                Author = embed.Author,
                AuthorUrl = embed.AuthorUrl,
                AuthorIcon = embed.AuthorIcon,
                Image = embed.Image,
                Thumbnail = embed.Thumbnail,
                AddTimestamp = embed.AddTimestamp,
                Color = embed.Color,
                Fields = embed.Fields.Select(ToContract).ToList()
            };
        }

        private EmbedFieldResponse ToContract(Entities.EmbedField field)
        {
            return new EmbedFieldResponse
            {
                Id = field.Id,
                Name = field.Name,
                Value = field.Value,
                Inline = field.Inline
            };
        }

        #endregion
    }
}
