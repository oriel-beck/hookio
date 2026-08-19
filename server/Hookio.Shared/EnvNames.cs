namespace Hookio.Shared;

/// <summary>Documented environment variable names. Values come from the process environment (compose env_file / host). Never bake secrets into the image.</summary>
public static class EnvNames
{
    public const string JwtSecret = "JWT_SECRET";
    public const string PgConnectionString = "PG_CONNECTION_STRING";
    public const string RedisConnectionString = "REDIS_CONNECTION_STRING";
    public const string DiscordClientId = "DISCORD_CLIENT_ID";
    public const string DiscordClientSecret = "DISCORD_CLIENT_SECRET";
    public const string DiscordRedirectUri = "DISCORD_REDIRECT_URI";
    public const string TwitchClientId = "TWITCH_CLIENT_ID";
    public const string TwitchClientSecret = "TWITCH_CLIENT_SECRET";
    public const string TwitchEventSubSecret = "TWITCH_EVENTSUB_SECRET";
    public const string TwitchEventSubCallbackUrl = "TWITCH_EVENTSUB_CALLBACK_URL";
    public const string CookieSecure = "HOOKIO_COOKIE_SECURE";
}
