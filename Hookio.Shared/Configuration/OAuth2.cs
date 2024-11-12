
namespace Hookio.Shared.Configuration
{
    public class OAuth2
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string RedirectURI { get; set; }
        public required string Scopes { get; set; }
    }
}
