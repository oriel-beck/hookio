namespace Hookio.DataManagers.Interfaces
{
    public interface IYouTubeManager
    {
        Task<HttpResponseMessage?> Subscribe(string channel_id, CancellationToken cancellationToken);
    }
}