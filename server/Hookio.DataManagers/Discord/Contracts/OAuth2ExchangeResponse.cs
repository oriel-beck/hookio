using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts
{
    public class OAuth2ExchangeResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "";

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = "";

        /// <summary>Seconds until the access token expires (Discord documents 604800 = 7 days).</summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "";
    }
}
