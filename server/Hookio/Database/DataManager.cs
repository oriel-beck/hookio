using Discord;
using Discord.Rest;
using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database
{
    public class DataManager(IDbContextFactory<HookioContext> contextFactory) : IDataManager
    {
        private readonly HttpClient _http = new()
        {
            BaseAddress = new Uri("https://discord.com")
        };

        private static HttpRequestMessage DiscordRequestMessage(string accessToken, string url)
        {
            return new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = {
                    Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken)
                }

            };
        }

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
                    Id = guild.Id,
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
        public async Task<AnnouncementResponse?> GetAnnouncementById(int id)
        {
            var ctx = await contextFactory.CreateDbContextAsync();
            IQueryable<Announcement> query = ctx.Announcements;
            var announcement = await query.FirstOrDefaultAsync();
            return announcement is null ? null : new AnnouncementResponse()
            {
                Id = announcement.Id,
                AnnouncementType = announcement.AnnouncementType,
                Origin = announcement.Origin,
                Message = announcement.Message,
                // embed todo
            };
        }

        public async Task<AnnouncementResponse?> CreateAnnouncement(ulong guildId, AnnouncementRequest request)
        {
            var ctx = await contextFactory.CreateDbContextAsync();
            Announcement announcement = new()
            {
                GuildId = guildId,
                WebhookUrl = request.WebhookUrl,
                Origin = request.Origin,
                Message = request.Message,
                AnnouncementType = request.AnnouncementType,
                // embed tdod
            };
            ctx.Announcements.Add(announcement);
            await ctx.SaveChangesAsync();
            return new AnnouncementResponse()
            {
                Id = announcement.Id,
                AnnouncementType = announcement.AnnouncementType,
                Origin = announcement.Origin,
                Message = announcement.Message,
                // embed todo
            };

        }
        #endregion
    }
}
