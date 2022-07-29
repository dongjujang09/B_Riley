using Microsoft.Extensions.Caching.Memory;

namespace B_Riley.BankingApp.Utils
{

    public class AppCache : IAppCache
    {
        private readonly int cacheTimespan;         // in sec
        private readonly IMemoryCache memoryCache;

        public AppCache(IMemoryCache memoryCache, int cacheTimespan = 60)
        {
            this.memoryCache = memoryCache;
            this.cacheTimespan = cacheTimespan;
        }


        public T Get<T>(string key)
        {
            if (memoryCache.TryGetValue(key, out T cacheValue))
                return cacheValue;
            
            return default;
        }

        public void Set<T>(string key, T value)
        {
            memoryCache.Set(key, value, DateTimeOffset.Now.AddSeconds(cacheTimespan));
        }

        public void Remove(string key)
        {
            memoryCache.Remove(key);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, int timespan, Func<Task<T>> func)
        {
            var cachedValue = await memoryCache.GetOrCreate(key, cacheEntry =>
            {
               cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(timespan);
               return func();
            });
            return cachedValue;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> func)
        {
            return await GetOrCreateAsync(key, cacheTimespan, func);
        }

        public T GetOrCreate<T>(string key, int timespan, Func<T> func)
        {
            var cachedValue = memoryCache.GetOrCreate(key, cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(timespan);
                return func();
            });
            return cachedValue;
        }

        public T GetOrCreate<T>(string key, Func<T> func)
        {
            return GetOrCreate(key, cacheTimespan, func);
        }
    }
}