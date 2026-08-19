using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hookio.Database;
using Hookio.Exceptions;
using Hookio.Shared;
using Hookio.Twitch.Contracts;
using Hookio.Twitch.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hookio.Twitch;

public class TwitchEventSubService(
    ILogger<TwitchEventSubService> logger,
    IHttpClientFactory httpClientFactory) : ITwitchEventSubService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly SemaphoreSlim TokenLock = new(1, 1);
    private static string? _appToken;
    private static DateTimeOffset _appTokenExpiresAt = DateTimeOffset.MinValue;

    public async Task<TwitchEventSubRegistration> RegisterAsync(string channelUrl, CancellationToken cancellationToken = default)
    {
        var login = SubscriptionService.TwitchLoginFromUrl(channelUrl)
            ?? throw new InvalidChannelURLException("Invalid Twitch channel URL");

        var clientId = Require(EnvNames.TwitchClientId);
        var callback = Require(EnvNames.TwitchEventSubCallbackUrl);
        var secret = Require(EnvNames.TwitchEventSubSecret);
        if (secret.Length is < 10 or > 100)
            throw new FailedToSubscribeException("TWITCH_EVENTSUB_SECRET must be 10-100 characters");

        var token = await GetAppTokenAsync(cancellationToken);
        var client = HelixClient(clientId, token);

        var users = await client.GetFromJsonAsync<TwitchHelixUsersResponse>($"users?login={Uri.EscapeDataString(login)}", JsonOptions, cancellationToken);
        var user = users?.Data.FirstOrDefault() ?? throw new InvalidChannelURLException("Twitch user not found");

        var types = new (string Type, string Version)[]
        {
            ("stream.online", "1"),
            ("stream.offline", "1"),
            ("channel.update", "2"),
        };

        var ids = new List<string>();
        try
        {
            foreach (var (type, version) in types)
            {
                var body = new TwitchEventSubCreateRequest
                {
                    Type = type,
                    Version = version,
                    Condition = new TwitchEventSubCondition { BroadcasterUserId = user.Id },
                    Transport = new TwitchEventSubTransport { Callback = callback, Secret = secret }
                };
                using var response = await client.PostAsJsonAsync("eventsub/subscriptions", body, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Twitch EventSub subscribe failed with {Status}", (int)response.StatusCode);
                    throw new FailedToSubscribeException("Failed to register Twitch EventSub");
                }
                var created = await response.Content.ReadFromJsonAsync<TwitchEventSubCreateResponse>(JsonOptions, cancellationToken);
                var id = created?.Data.FirstOrDefault()?.Id;
                if (!string.IsNullOrEmpty(id)) ids.Add(id);
            }
        }
        catch
        {
            await UnregisterAsync(string.Join(',', ids), cancellationToken);
            throw;
        }

        return new TwitchEventSubRegistration(user.Login, user.Id, ids);
    }

    public async Task UnregisterAsync(string? eventSubIdsCsv, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventSubIdsCsv)) return;
        string clientId;
        string token;
        try
        {
            clientId = Require(EnvNames.TwitchClientId);
            token = await GetAppTokenAsync(cancellationToken);
        }
        catch
        {
            return;
        }

        var client = HelixClient(clientId, token);
        foreach (var id in eventSubIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                using var response = await client.DeleteAsync($"eventsub/subscriptions?id={Uri.EscapeDataString(id)}", cancellationToken);
                _ = response;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete EventSub subscription");
            }
        }
    }

    private async Task<string> GetAppTokenAsync(CancellationToken cancellationToken)
    {
        await TokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_appToken is not null && DateTimeOffset.UtcNow < _appTokenExpiresAt)
                return _appToken;

            var clientId = Require(EnvNames.TwitchClientId);
            var clientSecret = Require(EnvNames.TwitchClientSecret);
            var client = httpClientFactory.CreateClient();
            var url = $"https://id.twitch.tv/oauth2/token?client_id={Uri.EscapeDataString(clientId)}&client_secret={Uri.EscapeDataString(clientSecret)}&grant_type=client_credentials";
            using var response = await client.PostAsync(url, content: null, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new FailedToSubscribeException("Failed to obtain Twitch app access token");
            var token = await response.Content.ReadFromJsonAsync<TwitchTokenResponse>(JsonOptions, cancellationToken)
                ?? throw new FailedToSubscribeException("Failed to obtain Twitch app access token");
            _appToken = token.AccessToken;
            _appTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, token.ExpiresIn - 60));
            return _appToken;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    private HttpClient HelixClient(string clientId, string token)
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.twitch.tv/helix/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Client-Id", clientId);
        return client;
    }

    private static string Require(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new FailedToSubscribeException($"{name} is not configured");
        return value;
    }
}
