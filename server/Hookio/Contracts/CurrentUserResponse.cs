namespace Hookio.Contracts
{
    public class CurrentUserResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }
}
