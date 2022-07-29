namespace B_Riley.BankingApp.Utils
{
    public interface IAppCache
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Remove(string key);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> func);
        Task<T> GetOrCreateAsync<T>(string key, int timespan, Func<Task<T>> func);
        T GetOrCreate<T>(string cacheKey, Func<T> value);
        T GetOrCreate<T>(string cacheKey, int timespan, Func<T> value);
    }
}