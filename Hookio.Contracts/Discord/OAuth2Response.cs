using System.Text.Json.Serialization;

namespace Hookio.Contracts.Discord
{
    public class OAuth2Response
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public required string RefreshToken { get; set; }

        public required string Scope { get; set; }
    }
}
