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
        // TODO: make a Server class for this
        public async Task<List<DiscordGuildResponse>> GetUserServers(string accessToken)
        {
            var httpRequest = await _http.SendAsync(DiscordRequestMessage(accessToken!, "/api/v10/users/@me/guilds"));
            var servers = await httpRequest.Content.ReadFromJsonAsync<List<DiscordGuildResponse>>();
            servers ??= [];
            return servers;
        }

        public async Task<CurrentUserResponse?> GetUser(string userId) 
        {
            // TODO: change this to use the database for getting the user data, save user data on login (avatar, id, etc)
            var accessToken = await GetAccessToken(userId);
            var httpResponse = await _http.SendAsync(DiscordRequestMessage(accessToken!, "/api/v10/users/@me"));
            var user = await httpResponse.Content.ReadFromJsonAsync<DiscordCurrentUserResponse?>();
            return await ToContract(user, accessToken!);
        }

        public async Task<User?> CreateUser(DiscordCurrentUserResponse user, DiscordTokenResponse token)
        {
            var ctx = contextFactory.CreateDbContext();

            var newUser = new User
            {
                AccessToken = token.AccessToken,
                ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(token.ExpiresIn),
                Id = user.Id,
                RefreshToken = token.RefreshToken,
            };
            var currentUser = ctx.Users.SingleOrDefault(u => u.Id == user.Id);
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

        public async Task<string?> GetAccessToken(string userId)
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
                    { "client_id", Environment.GetEnvironmentVariable("CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("CLIENT_SECRET")! },
                    { "refresh_token", currentUser.RefreshToken }
                }
                ));
                var result = await discordResponse.Content.ReadFromJsonAsync<DiscordTokenResponse>();
                currentUser.ExpireAt = DateTimeOffset.UtcNow.AddMilliseconds(result.ExpiresIn);
                currentUser.AccessToken = result.AccessToken;
                await ctx.SaveChangesAsync();
                return result.AccessToken;
            }
            else
            {
                return currentUser.AccessToken;
            }
        }

        private async Task<CurrentUserResponse?> ToContract(DiscordCurrentUserResponse? user, string accessToken)
        {
            if (user == null) return null;
            int.TryParse(user.Discriminator, out int numberDiscriminator);
            long.TryParse(user.Id, out long numberId);
            var avatarSuffix = user.Avatar?.StartsWith("a_") is true ? "gif" : "png";
            var currentUser = new CurrentUserResponse()
            {
                Discriminator = user.Discriminator,
                Id = user.Id,
                UserName = user.GlobalName is null ? user.UserName : user.GlobalName,
                // TODO: implement premium in the DB
                Premium = 0,
                Avatar = user.Discriminator == "0"
                            ? user.Avatar is null ? $"https://cdn.discordapp.com/embed/avatars/{(numberId >> 22) % 6}.png" : $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.{avatarSuffix}"
                            : user.Avatar is null ? $"https://cdn.discordapp.com/embed/avatars/{numberDiscriminator % 5}.png" : $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.{avatarSuffix}",
                Guilds = (await GetUserServers(accessToken)).Where(guild =>
                {
                    var _ = long.TryParse(guild.Permissions, out long permission);
                    return (uint)(permission & 0x0000000000000020) == 0x0000000000000020;
                }).Select(guild => new GuildResponse()
                {
                    Name = guild.Name,
                    // TODO: fallback image if there is none
                    Icon = guild.Icon is not null ? guild.Icon.StartsWith("a_") ? $"https://cdn.discordapp.com/icons/{guild.Id}/{guild?.Icon}.gif" : $"https://cdn.discordapp.com/icons/{guild.Id}/{guild?.Icon}.png" : "fallback"
                }).ToList()
            };
            return currentUser;
        }
        #endregion
    }
}
