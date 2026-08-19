using System.Text.Json.Serialization;

namespace Hookio.Twitch.Contracts;

public sealed class TwitchTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public sealed class TwitchHelixUsersResponse
{
    [JsonPropertyName("data")]
    public List<TwitchHelixUser> Data { get; set; } = [];
}

public sealed class TwitchHelixUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("login")]
    public string Login { get; set; } = "";

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

public sealed class TwitchEventSubCreateRequest
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("condition")]
    public required TwitchEventSubCondition Condition { get; set; }

    [JsonPropertyName("transport")]
    public required TwitchEventSubTransport Transport { get; set; }
}

public sealed class TwitchEventSubCondition
{
    [JsonPropertyName("broadcaster_user_id")]
    public required string BroadcasterUserId { get; set; }
}

public sealed class TwitchEventSubTransport
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "webhook";

    [JsonPropertyName("callback")]
    public required string Callback { get; set; }

    [JsonPropertyName("secret")]
    public required string Secret { get; set; }
}

public sealed class TwitchEventSubCreateResponse
{
    [JsonPropertyName("data")]
    public List<TwitchEventSubCreated> Data { get; set; } = [];
}

public sealed class TwitchEventSubCreated
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
}

public sealed class TwitchEventSubCallback
{
    [JsonPropertyName("challenge")]
    public string? Challenge { get; set; }

    [JsonPropertyName("subscription")]
    public TwitchEventSubEnvelope? Subscription { get; set; }

    [JsonPropertyName("event")]
    public TwitchEventSubEvent? Event { get; set; }
}

public sealed class TwitchEventSubEnvelope
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public sealed class TwitchEventSubEvent
{
    [JsonPropertyName("broadcaster_user_id")]
    public string? BroadcasterUserId { get; set; }

    [JsonPropertyName("broadcaster_user_login")]
    public string? BroadcasterUserLogin { get; set; }

    [JsonPropertyName("broadcaster_user_name")]
    public string? BroadcasterUserName { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("category_name")]
    public string? CategoryName { get; set; }
}
