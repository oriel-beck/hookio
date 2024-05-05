using Discord;
using Discord.Rest;
using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Hookio.Discord;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Enunms;
using Hookio.Exceptions;
using Hookio.Youtube.Contracts;
using Hookio.Youtube.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;

namespace Hookio.Database
{
    public class DataManager(ILogger<DataManager> logger, IDbContextFactory<HookioContext> contextFactory, IConnectionMultiplexer connectionMultiplexer, IYoutubeService youtubeService, DiscordRequestManager discordRequestManager) : IDataManager
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly IDatabase _redisDatabase = connectionMultiplexer.GetDatabase();
        
        #region users
        public async Task<IEnumerable<DiscordPartialGuild>?> GetUserGuilds(User user) =>
            await discordRequestManager.GetDiscordUserGuilds(user.AccessToken);

        public async Task<CurrentUserResponse?> GetUser(ulong userId)
        {
            var ctx = await contextFactory.CreateDbContextAsync();
            var dbUser = await ctx.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (dbUser == null) return null;

            var accessToken = await GetAccessToken(userId);
            if (accessToken == null) return null;

            var discordUser = await discordRequestManager.GetDiscordUser(accessToken);
            if (discordUser == null) return null;

            return await ToContract(dbUser, discordUser);
        }

        public async Task<User> CreateUser(DiscordSelfUser user, OAuth2ExchangeResponse token)
        {
            var ctx = contextFactory.CreateDbContext();
            var currentUser = ctx.Users.SingleOrDefault(u => u.Id == user.Id);
            if (currentUser == null)
            {
                var newUser = new User
                {
                    AccessToken = token.AccessToken,
                    ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(token.ExpiresIn),
                    Id = user.Id,
                    RefreshToken = token.RefreshToken,
                    Email = user.Email,
                };
                await ctx.Users.AddAsync(newUser);
                return newUser;
            }
            else
            {
                currentUser.RefreshToken = token.RefreshToken;
                currentUser.AccessToken = token.AccessToken;
                currentUser.ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(token.ExpiresIn);
                await ctx.SaveChangesAsync();
                return currentUser;
            }
        }

        public async Task<CurrentUserResponse?> Authenticate(HttpContext httpContext, string code)
        {
            var context = await contextFactory.CreateDbContextAsync();
            try
            {
                var result = await discordRequestManager.ExchangeOAuth2Code(code);
                if (result == null) return null;


                var discordUser = await discordRequestManager.GetDiscordUser(result.AccessToken);
                if (discordUser == null) return null;

                var dbUser = await CreateUser(discordUser, result);
                var currentUser = await ToContract(dbUser, discordUser);

                CreateTokenAndSetCookie(httpContext, discordUser, currentUser?.Guilds.Select(g => g.Id));

                return currentUser;
            }
            catch (Exception ex)
            {
                logger.LogError("[{FunctionName}]: '{Message}'", nameof(Authenticate), ex.Message);
                return null;
            }
        }

        public async Task<string?> GetAccessToken(ulong userId)
        {
            var ctx = contextFactory.CreateDbContext();
            var currentUser = ctx.Users.SingleOrDefault(u => u.Id == userId);
            if (currentUser == null) return null;
            if (currentUser.ExpireAt > DateTimeOffset.UtcNow) return currentUser.AccessToken;
            // generate a new access token and update the db
            var result = await discordRequestManager.RefreshOAuth2(currentUser.RefreshToken);
            currentUser.ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(result!.ExpiresIn);
            currentUser.AccessToken = result.AccessToken;
            currentUser.RefreshToken = result.RefreshToken;
            await ctx.SaveChangesAsync();
            return result.AccessToken;
        }

        public async Task RefreshUserAuthentication(TokenValidatedContext context, SecurityToken token)
        {
            var parsedToken = _tokenHandler.ReadJwtToken(token.UnsafeToString());
            var ctx = await contextFactory.CreateDbContextAsync();
            var claim = parsedToken.Claims.First(claim => claim.Type == "id").Value;
            _ = ulong.TryParse(claim, out var userId);
            var accessToken = await GetAccessToken(userId);
            if (accessToken == null) return;

            var discordUser = await discordRequestManager.GetDiscordUser(accessToken);
            if (discordUser == null) return;

            var dbUser = await ctx.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (dbUser == null) return;

            var userContract = await ToContract(dbUser, discordUser);
            CreateTokenAndSetCookie(context, discordUser, userContract!.Guilds.Select(g => g.Id));
        }

        public void CreateTokenAndSetCookie(TokenValidatedContext context, DiscordSelfUser user, IEnumerable<string>? guildIds)
        {
            var claims = new Claim[]
            {
                new("id", user.Id.ToString()),
                new("guilds", JsonConvert.SerializeObject(guildIds))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);
            context.Response.Cookies.Append("Authorization", tokenString, new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(3),
                SameSite = SameSiteMode.Strict
            });
        }

        public void CreateTokenAndSetCookie(HttpContext context, DiscordSelfUser user, IEnumerable<string>? guildIds)
        {
            var claims = new Claim[]
            {
                new("id", user.Id.ToString()),
                new("guilds", JsonConvert.SerializeObject(guildIds))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);
            context.Response.Cookies.Append("Authorization", tokenString, new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(3),
                SameSite = SameSiteMode.Strict
            });
        }
        #endregion

        #region subscriptions
        public async Task<SubscriptionResponse?> GetSubscription(ulong guildId, int id)
        {
            var ctx = await contextFactory.CreateDbContextAsync();

            var subscription = await ctx.Subscriptions
                .Where(x =>
                    (x.GuildId == guildId) &&
                    (x.Id == id))
                .Include(s => s.Events)
                    .ThenInclude(e => e.Message)
                        .ThenInclude(m => m!.Embeds.OrderBy(e => e.Index))
                            .ThenInclude(e => e.Fields.OrderBy(f => f.Index))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null)
            {
                return null; // Subscription not found
            }

            return new SubscriptionResponse
            {
                Id = subscription.Id,
                SubscriptionType = subscription.SubscriptionType,
                Url = subscription.Url,
                Events = subscription.Events.Select(ToContract).ToDictionary(key => key.EventType),
                GuildId = subscription.GuildId,
            };
        }

        public async Task<Subscription?> GetSubscription(int id)
        {
            var ctx = await contextFactory.CreateDbContextAsync();

            var subscription = await ctx.Subscriptions
                .Where(x => x.Id == id)
                .Include(s => s.Events)
                    .ThenInclude(e => e.Message)
                        .ThenInclude(m => m!.Embeds.OrderBy(e => e.Index))
                            .ThenInclude(e => e.Fields.OrderBy(f => f.Index))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null)
            {
                return null; // Subscription not found
            }

            return subscription;
        }

        // TODO: Maybe add buttons at some point
        public async Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request)
        {
            string? channelId = null;
            if (request.SubscriptionType == SubscriptionType.Youtube)
            {
                channelId = youtubeService.GetYoutubeChannelId(request!.Url) ?? throw new Exception("Invalid youtube channel ID");
            }
            using var context = await contextFactory.CreateDbContextAsync();
            var subscriptionsCount = await context.Subscriptions.Where(x => x.GuildId == guildId).CountAsync();
            if (subscriptionsCount >= 2)
            {
                // TODO: Premium only, need a Guild entity for that
                throw new RequiresPremiumException("This feature requires premium");
            }

            using var transaction = await context.Database.BeginTransactionAsync();
            int length = 0;
            try
            {
                var subscription = new Subscription
                {
                    GuildId = guildId,
                    WebhookUrl = request.WebhookUrl,
                    Url = request.Url,
                    SubscriptionType = request.SubscriptionType,
                };
                context.Subscriptions.Add(subscription);

                foreach (var eventRequest in request.Events)
                {

                    var eventEntity = new Event
                    {
                        Type = eventRequest.Key,
                        Subscription = subscription // Set Subscription navigation property
                    };

                    context.Events.Add(eventEntity);
                    await context.SaveChangesAsync(); // SaveChangesAsync to generate EventId

                    var message = new Message
                    {
                        Content = eventRequest.Value.Message.Content,
                        WebhookUsername = eventRequest.Value.Message.Username,
                        WebhookAvatar = eventRequest.Value.Message.Avatar,
                        Event = eventEntity, // Set Event navigation property
                    };
                    context.Messages.Add(message);
                    length += eventRequest.Value.Message.Content.Length;

                    eventEntity.Message = message;

                    await context.SaveChangesAsync(); // SaveChangesAsync to generate SubscriptionId

                    foreach (var embedRequest in eventRequest.Value.Message.Embeds)
                    {
                        var embed = new Entities.Embed
                        {
                            Index = embedRequest.Index,
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
                                Index = embedFieldRequest.Index,
                                Name = embedFieldRequest.Name,
                                Value = embedFieldRequest.Value,
                                Inline = embedFieldRequest.Inline,
                                Embed = embed // Set Embed navigation property
                            };
                            context.EmbedFields.Add(embedField);
                        }
                    }
                }

                if (length > 6000)
                {
                    await transaction.RollbackAsync();
                    throw new EmbedTooLongException("The embeds and content length cannot be longer than 6k characters");
                }

                await context.SaveChangesAsync(); // SaveChangesAsync to generate EmbedFieldIds

                await transaction.CommitAsync();

                if (request.SubscriptionType == SubscriptionType.Youtube)
                {
                    var success = await youtubeService.Subscribe(channelId!);
                    if (!(bool)success) throw new Exception("Failed to subscribe to the channel ID");
                }

                var newSubscription = await context.Subscriptions
                    .Where(x => x.Id == subscription.Id)
                    .Include(x => x.Events)
                    .ThenInclude(e => e.Message)
                    .ThenInclude(m => m.Embeds)
                    .ThenInclude(e => e.Fields)
                    .SingleAsync();

                return ToContract(newSubscription);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception for handling at a higher level
            }
        }

        public async Task<SubscriptionResponse?> UpdateSubscription(ulong guildId, int id, SubscriptionRequest request)
        {
            string? channelId = null;
            if (request.SubscriptionType == SubscriptionType.Youtube)
            {
                channelId = youtubeService.GetYoutubeChannelId(request!.Url) ?? throw new Exception("Invalid youtube channel ID");
            }
            using var context = await contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            int length = 0;
            try
            {
                // Get current subscription
                var subscription = await context.Subscriptions.Where(x =>
                        (x.Id == id) &&
                        (x.GuildId == guildId))
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return null;
                }

                if (subscription.Url != request.Url)
                {
                    // TODO: unsubscribe from old, subscribe to new
                    if (subscription.SubscriptionType == SubscriptionType.Youtube)
                    {
                        await youtubeService.Subscribe(channelId!, false);
                    }
                }

                if (!string.IsNullOrEmpty(request.WebhookUrl))
                {
                    subscription.WebhookUrl = request.WebhookUrl;
                }

                var events = await context.Events
                    .Where(x => x.SubscriptionId == id)
                        .Include(x => x.Message)
                            .ThenInclude(x => x.Embeds)
                                .ThenInclude(x => x.Fields)
                    .ToDictionaryAsync(k => k.Id);

                foreach (var eventRequest in request.Events)
                {
                    // Get the current event of this event type
                    var _ = events.TryGetValue((int)eventRequest.Value.Id!, out var currentEvent);

                    if (currentEvent == null)
                    {
                        var newEvent = new Event
                        {
                            Type = eventRequest.Value.EventType,
                            Subscription = subscription,
                        };
                        context.Events.Add(newEvent);
                        await context.SaveChangesAsync(); // Save to generate EventId
                        currentEvent = newEvent;
                    }

                    // Update the current event based on incoming data
                    var incomingMessage = eventRequest.Value.Message;
                    currentEvent.Message.Content = incomingMessage.Content;
                    currentEvent.Message.WebhookAvatar = incomingMessage.Avatar;
                    currentEvent.Message.WebhookUsername = incomingMessage.Username;

                    // Find all embeds that were not provided in this update request, delete them later
                    List<Entities.Embed> notFoundEmbeds = currentEvent.Message.Embeds.Where(embed => !incomingMessage.Embeds.Any(req => req.Id == embed.Id))
                                          .Select(embed => embed)
                                          .ToList();

                    length += eventRequest.Value.Message.Content.Length;

                    foreach (var incomingEmbed in incomingMessage.Embeds)
                    {
                        // Get current embed from the database if possible
                        var currentEmbed = currentEvent.Message.Embeds.Find(embed => embed.Id == incomingEmbed.Id);
                        if (currentEmbed is null)
                        {
                            // If there is no embed with this ID, create a new one
                            var newEmbed = new Entities.Embed
                            {
                                Index = incomingEmbed.Index,
                                Author = incomingEmbed.Author,
                                AuthorUrl = incomingEmbed.AuthorUrl,
                                AuthorIcon = incomingEmbed.AuthorIcon,
                                Title = incomingEmbed.Title,
                                TitleUrl = incomingEmbed.TitleUrl,
                                Description = incomingEmbed.Description,
                                Image = incomingEmbed.Image,
                                Thumbnail = incomingEmbed.Thumbnail,
                                Color = incomingEmbed.Color,
                                Footer = incomingEmbed.Footer,
                                FooterIcon = incomingEmbed.FooterIcon,
                                AddTimestamp = incomingEmbed.AddTimestamp,
                                Message = currentEvent.Message // Set Message naviagation property
                            };

                            context.Embeds.Add(newEmbed);
                            await context.SaveChangesAsync(); // Save to generate EmbedId

                            // This also adds field lengths
                            length += incomingEmbed.Length;

                            // Set the new embed as the current one
                            currentEmbed = newEmbed;
                        }
                        else
                        {
                            // If there is an embed with this Id, upadte it and all of its fields
                            currentEmbed.Author = incomingEmbed.Author;
                            currentEmbed.AuthorUrl = incomingEmbed.AuthorUrl;
                            currentEmbed.AuthorIcon = incomingEmbed.AuthorIcon;
                            currentEmbed.Title = incomingEmbed.Title;
                            currentEmbed.TitleUrl = incomingEmbed.TitleUrl;
                            currentEmbed.Description = incomingEmbed.Description;
                            currentEmbed.Image = incomingEmbed.Image;
                            currentEmbed.Thumbnail = incomingEmbed.Thumbnail;
                            currentEmbed.Color = incomingEmbed.Color;
                            currentEmbed.Footer = incomingEmbed.Footer;
                            currentEmbed.FooterIcon = incomingEmbed.FooterIcon;
                            currentEmbed.AddTimestamp = incomingEmbed.AddTimestamp;
                            currentEmbed.Index = incomingEmbed.Index;

                            length += Util.GetEmbedLength(currentEmbed);
                        }

                        // If there are any fields missing in this update that existed before, delete them.
                        List<Entities.EmbedField> notFoundFields = (currentEmbed.Fields is null ? [] : currentEmbed.Fields).Where(field => !incomingEmbed.Fields.Any(req => req.Id == field.Id))
                            .Select(field => field)
                            .ToList();

                        foreach (var incomingEmbedField in incomingEmbed.Fields)
                        {
                            var currentEmbedField = (currentEmbed.Fields is null ? [] : currentEmbed.Fields).Find(embed => embed.Id == incomingEmbedField.Id);

                            if (currentEmbedField is null)
                            {
                                var newField = new Entities.EmbedField
                                {
                                    Index = incomingEmbedField.Index,
                                    Name = incomingEmbedField.Name,
                                    Value = incomingEmbedField.Value,
                                    Inline = incomingEmbedField.Inline,
                                    Embed = currentEmbed // Set Embed navigation property
                                };

                                context.EmbedFields.Add(newField);
                            }
                            else
                            {
                                currentEmbedField.Index = incomingEmbedField.Index;
                                currentEmbedField.Name = incomingEmbedField.Name;
                                currentEmbedField.Value = incomingEmbedField.Value;
                                currentEmbedField.Inline = incomingEmbedField.Inline;

                                length += currentEmbedField.Name.Length + currentEmbedField.Value.Length;
                            }
                        }

                        foreach (var notFoundField in notFoundFields)
                        {
                            context.EmbedFields.Remove(notFoundField);
                        }
                    }

                    foreach (var notFoundEmbed in notFoundEmbeds)
                    {
                        context.Embeds.Remove(notFoundEmbed);
                    }
                }

                if (length > 6000)
                {
                    await transaction.RollbackAsync();
                    throw new EmbedTooLongException();
                }

                await context.SaveChangesAsync(); // SaveChangesAsync to generate EmbedFieldIds

                await transaction.CommitAsync();

                if (request.SubscriptionType == SubscriptionType.Youtube)
                {
                    var success = await youtubeService.Subscribe(channelId!);
                    if (!(bool)success) throw new Exception("Failed to subscribe to the channel ID");
                }

                return new SubscriptionResponse
                {
                    Id = subscription.Id,
                    SubscriptionType = subscription.SubscriptionType,
                    Url = subscription.Url,
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception for handling at a higher level
            }
        }

        public async Task<GuildSubscriptionsResponse> GetSubscriptions(ulong guildId, SubscriptionType? provider, bool withCounts = false)
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

            query = query.Include(x => x.Events).ThenInclude(e => e.Message).ThenInclude(m => m.Embeds).ThenInclude(e => e.Fields);

            var subscriptions = (await query.ToListAsync()).Select(subscription => new SubscriptionResponse
            {
                Id = subscription.Id,
                SubscriptionType = subscription.SubscriptionType,
                Url = subscription.Url,
                GuildId = guildId,
                Events = subscription.Events.Select(ToContract).ToDictionary(key => key.EventType)
            });

            return new GuildSubscriptionsResponse
            {
                Count = await ctx.Subscriptions.Where(s => s.GuildId == guildId).CountAsync(),
                Subscriptions = subscriptions.ToList()
            };
        }

        public async Task<List<Subscription>> GetSubscriptions(YoutubeNotification notification, EventType eventType)
        {
            var context = await contextFactory.CreateDbContextAsync();
            return await context.Subscriptions.Where(s => s.Url == $"https://youtube.com/channel/{notification.ChannelId}")
                .Include(x => x.Events.Where(e => e.Type == eventType))
                .ThenInclude(ev => ev.Message)
                .ThenInclude(m => m.Embeds)
                .ThenInclude(em => em.Fields)
                .ToListAsync();
        }

        // TODO: DeleteSubscription
        #endregion

        #region contracts
        private async Task<CurrentUserResponse> ToContract(User dbUser, DiscordSelfUser discordUser)
        {
            var res = await GetUserGuilds(dbUser);
            var guilds = res!.Where(guild => (guild!.Permissions & 0x0000000000000020) != 0).Select(guild => new GuildResponse()
            {
                Id = guild!.Id.ToString(),
                Name = guild.Name,
                Icon = guild.IconUrl,
            });

            var currentUser = new CurrentUserResponse()
            {
                Discriminator = discordUser.Discriminator,
                Id = discordUser.Id,
                Username = discordUser.GlobalName is null ? discordUser.Username : discordUser.GlobalName,
                // TODO: implement premium in the DB, patreon tiers = guild amounts, the Guild itself will contain the unlocked features for each tier
                Premium = 0,
                Avatar = discordUser.GetAvatarUrl(),
                Guilds = guilds
            };
            return currentUser;
        }

        private SubscriptionResponse ToContract(Subscription subscription)
        {
            return new SubscriptionResponse
            {
                Url = subscription.Url,
                Id = subscription.Id,
                SubscriptionType = subscription.SubscriptionType,
                GuildId = subscription.GuildId,
                Events = subscription.Events.Select(ToContract).ToDictionary(ev => ev.EventType)
            };
        }

        private MessageResponse ToContract(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                Content = message.Content,
                Embeds = message.Embeds.Select(ToContract).ToList(),
                Avatar = message.WebhookAvatar,
                Username = message.WebhookUsername,
            };
        }

        private EmbedResponse ToContract(Entities.Embed embed)
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

        private EventResponse ToContract(Event eventEntity)
        {
            return new EventResponse
            {
                Id = eventEntity.Id,
                EventType = eventEntity.Type,
                Message = ToContract(eventEntity.Message),
            };
        }
        #endregion
    }
}
