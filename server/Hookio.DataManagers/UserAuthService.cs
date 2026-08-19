using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Hookio.Database;

public class UserAuthService(
    ILogger<UserAuthService> logger,
    IDbContextFactory<HookioContext> contextFactory,
    IDiscordRequestManager discordRequestManager,
    IConnectionMultiplexer redis) : IUserAuthService
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly IDatabase _cache = redis.GetDatabase();
    private static readonly TimeSpan GuildsCacheTtl = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new();

    public async Task<CurrentUserResponse?> GetUser(ulong userId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        await RevalidateUserAccessToken(userId);

        var dbUser = await ctx.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
        if (dbUser == null) return null;

        var discordUser = await discordRequestManager.GetDiscordUser(userId);
        if (discordUser == null) return null;

        return await ToContract(dbUser, discordUser);
    }

    public async Task<User> CreateUser(DiscordSelfUser user, OAuth2ExchangeResponse token)
    {
        using var ctx = contextFactory.CreateDbContext();
        var currentUser = ctx.Users.SingleOrDefault(u => u.Id == user.Id);
        if (currentUser == null)
        {
            var newUser = new User
            {
                AccessToken = token.AccessToken,
                ExpireAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn),
                Id = user.Id,
                RefreshToken = token.RefreshToken,
                Email = user.Email!,
            };
            await ctx.Users.AddAsync(newUser);
            currentUser = newUser;
        }
        else
        {
            currentUser.RefreshToken = token.RefreshToken;
            currentUser.AccessToken = token.AccessToken;
            currentUser.ExpireAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
        }
        await ctx.SaveChangesAsync();
        return currentUser;
    }

    public async Task<CurrentUserResponse?> Authenticate(HttpContext httpContext, string code)
    {
        try
        {
            var result = await discordRequestManager.ExchangeOAuth2Code(code);
            if (result == null) return null;

            var discordUser = await discordRequestManager.GetDiscordUser(result.AccessToken);
            if (discordUser == null) return null;

            var dbUser = await CreateUser(discordUser, result);
            await _cache.KeyDeleteAsync(GuildsCacheKey(dbUser.Id));
            var currentUser = await ToContract(dbUser, discordUser);

            CreateTokenAndSetCookie(httpContext, discordUser, currentUser?.Guilds.Select(g => g.Id));

            return currentUser;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Authenticate failed");
            return null;
        }
    }

    public async Task RevalidateUserAccessToken(ulong userId)
    {
        using var ctx = contextFactory.CreateDbContext();
        var currentUser = await ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (currentUser == null) return;
        if (currentUser.ExpireAt > DateTimeOffset.UtcNow.AddDays(1)) return;
        var result = await discordRequestManager.RefreshOAuth2(userId);
        if (result is null) return;
        currentUser.ExpireAt = DateTimeOffset.UtcNow.AddSeconds(result.ExpiresIn);
        currentUser.AccessToken = result.AccessToken;
        currentUser.RefreshToken = result.RefreshToken;
        await ctx.SaveChangesAsync();
    }

    private async Task<CurrentUserResponse> ToContract(User dbUser, DiscordSelfUser discordUser)
    {
        var guilds = await GetUserGuildsCached(dbUser);
        return SubscriptionMapper.ToCurrentUser(discordUser, SubscriptionMapper.ToGuilds(guilds ?? []));
    }

    private async Task<IEnumerable<DiscordPartialGuild>?> GetUserGuildsCached(User user)
    {
        var key = GuildsCacheKey(user.Id);
        var cached = await _cache.StringGetAsync(key);
        if (cached.HasValue)
        {
            try
            {
                return JsonSerializer.Deserialize<List<DiscordPartialGuild>>(cached.ToString()!, JsonOptions);
            }
            catch (JsonException)
            {
                await _cache.KeyDeleteAsync(key);
            }
        }

        var guilds = await discordRequestManager.GetDiscordUserGuilds(user.AccessToken);
        if (guilds is not null)
        {
            await _cache.StringSetAsync(key, JsonSerializer.Serialize(guilds.ToList(), JsonOptions), GuildsCacheTtl);
        }
        return guilds;
    }

    private void CreateTokenAndSetCookie(HttpContext context, DiscordSelfUser user, IEnumerable<string>? guildIds)
    {
        var expires = DateTime.UtcNow.AddHours(3);
        var claims = new Claim[]
        {
            new(AuthConstants.IdClaim, user.Id.ToString()),
            new(AuthConstants.GuildsClaim, JsonSerializer.Serialize(guildIds ?? []))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = AuthConstants.Issuer,
            Audience = AuthConstants.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(EnvNames.JwtSecret)!)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);
        context.Response.Cookies.Append(AuthConstants.CookieName, tokenString, UtilCookie.AuthCookieOptions(context, expires));
    }

    private static string GuildsCacheKey(ulong userId) => $"discord:guilds:{userId}";
}
