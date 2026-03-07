using Microsoft.Extensions.Caching.Memory;

namespace MiNegocioCR.Api.Application.AI.Cache
{
    public class ResponseCache : IResponseCache
    {
        private readonly IMemoryCache _cache;

        public ResponseCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<string?> GetAsync(string key)
        {
            _cache.TryGetValue(key, out string? response);
            return Task.FromResult(response);
        }

        public Task SetAsync(string key, string response)
        {
            _cache.Set(key, response, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }
    }
}
