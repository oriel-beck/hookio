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
            return await ToUserContract(client);
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

        private async Task<CurrentUserResponse?> ToUserContract(DiscordRestClient client)
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

        public Task<bool> CanUserAccessGuild(ulong userId, ulong guildId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region announcements
        public async Task<SubscriptionResponse?> GetSubscriptionById(int id)
        {
            var ctx = await contextFactory.CreateDbContextAsync();

            var subscription = await ctx.Subscriptions
                .Include(s => s.Message)
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
                Message = MapToMessageResponse(subscription.Message)
            };
        }

        private MessageResponse? MapToMessageResponse(Message? message)
        {
            if (message == null)
            {
                return null; // Message not found
            }

            return new MessageResponse
            {
                Content = message.Content,
                Embeds = message.Embeds.Select(MapToEmbedResponse).ToList(),
                // TODO: Set Username and Avatar properties
            };
        }

        private EmbedResponse MapToEmbedResponse(Entities.Embed embed)
        {
            return new EmbedResponse
            {
                Description = embed.Description,
                Url = embed.Url,
                Title = embed.Title,
                Color = embed.Color,
                Image = embed.Image,
                Author = embed.Author,
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
                Name = field.Name,
                Value = field.Value,
                Inline = field.Inline
            };
        }

        // TODO: Add avatar and maybe buttons at some point
        public async Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request)
        {
            if (!IsMessageValid(request.Message))
            {
                return null;
            }

            using (var context = await contextFactory.CreateDbContextAsync())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var subscription = new Subscription
                        {
                            GuildId = guildId,
                            WebhookUrl = request.WebhookUrl,
                            ChannelId = request.ChannelId,
                            SubscriptionType = request.SubscriptionType
                        };
                        context.Subscriptions.Add(subscription);

                        var message = new Message
                        {
                            Content = request.Message.Content,
                            Subscription = subscription
                        };
                        context.Messages.Add(message);

                        await context.SaveChangesAsync(); // SaveChangesAsync to generate SubscriptionId

                        foreach (var embedRequest in request.Message.Embeds)
                        {
                            var embed = new Entities.Embed
                            {
                                Description = embedRequest.Description,
                                Url = embedRequest.Url,
                                Title = embedRequest.Title,
                                Color = embedRequest.Color,
                                Image = embedRequest.Image,
                                Author = embedRequest.Author,
                                AuthorIcon = embedRequest.AuthorIcon,
                                Thumbnail = embedRequest.Thumbnail,
                                Message = message // Set Message navigation property
                            };
                            context.Embeds.Add(embed);

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
                            }
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

            var subscriptions = (await query.ToListAsync()).Select(subscription => new SubscriptionResponse
            {
                Id = subscription.Id,
                AnnouncementType = subscription.SubscriptionType,
                ChannelId = subscription.ChannelId,
            });

            return [.. subscriptions];
        }

        private static bool IsMessageValid(MessageRequest message)
        {
            // TODO: validate if the embed is valid (has description/title/fields)
            int count = message.Content.Length;
            foreach (EmbedRequest embedRequest in message.Embeds) 
            {
                count += embedRequest.Length;
                foreach (EmbedFieldRequest embedField in embedRequest.Fields)
                {
                    if (embedField.Name.Length > EmbedFieldBuilder.MaxFieldNameLength) return false;
                    if (embedField.Value.Length >  EmbedFieldBuilder.MaxFieldValueLength) return false;
                }
            }
            if (count > 5500) return false;
            return true;
        }

        #endregion
    }
}
