using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace Hookio.Shared.Extensions
{
    public static class SessionExtensions
    {
        public static void SetWithExpiry<T>(this ISession session, string key, T value, TimeSpan? expiry)
        {
            var item = new
            {
                Value = value,
                Expiry = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null
            };
            session.SetString(key, JsonConvert.SerializeObject(item));
        }

        public static T? GetWithExpiry<T>(this ISession session, string key)
        {
            var jsonString = session.GetString(key);
            if (jsonString == null) return default;

            var item = JsonConvert.DeserializeObject<SessionItem<T>>(jsonString);
            if (item == null) return default;

            // Check for expiry
            if (item.Expiry.HasValue && DateTime.UtcNow > (DateTime)item.Expiry)
            {
                session.Remove(key);
                return default;
            }

            return item.Value;
        }

        private class SessionItem<T>
        {
            public required T Value { get; set; }
            public DateTime? Expiry { get; set; }
        }
    }
}
