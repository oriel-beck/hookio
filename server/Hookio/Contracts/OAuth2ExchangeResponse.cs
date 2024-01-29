﻿using System.Text.Json.Serialization;

namespace Hookio.Contracts
{
    public class OAuth2ExchangeResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
    }
}
