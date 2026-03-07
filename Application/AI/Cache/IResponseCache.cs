namespace MiNegocioCR.Api.Application.AI.Cache
{
    public interface IResponseCache
    {
        Task<string?> GetAsync(string key);

        Task SetAsync(string key, string response);
    }
}
