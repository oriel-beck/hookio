namespace Hookio.DataManagers.Interfaces
{
    public interface IVideosCacheManager
    {
        Task<ulong?> Get(string key);
        Task<bool> Set(string key, ulong value);
    }
}