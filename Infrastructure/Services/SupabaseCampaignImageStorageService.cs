using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Interfaces.Services;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class SupabaseCampaignImageStorageService : ICampaignImageStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabaseCampaignImageStorageService> _logger;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;

    public SupabaseCampaignImageStorageService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SupabaseCampaignImageStorageService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        var resolved = SupabaseStorageConfigurationReader.Read(configuration);
        _supabaseUrl = resolved.Url;
        _serviceKey = resolved.ServiceKey;
        _bucket = resolved.Bucket;
    }

    public async Task<string> UploadAsync(
        Guid businessId,
        Stream fileStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl))
            throw new InvalidOperationException("Supabase config missing: set Supabase:Url or SUPABASE_URL.");
        if (string.IsNullOrWhiteSpace(_serviceKey))
            throw new InvalidOperationException("Supabase config missing: set Supabase:ServiceKey or SUPABASE_SERVICE_ROLE_KEY.");

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".jpg";

        var path = $"campaign-images/{businessId}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var baseUrl = _supabaseUrl.TrimEnd('/');
        var uploadUrl = $"{baseUrl}/storage/v1/object/{_bucket}/{path}";
        var publicUrl = $"{baseUrl}/storage/v1/object/public/{_bucket}/{path}";

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60);
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
        request.Headers.Add("apikey", _serviceKey);
        request.Headers.Add("x-upsert", "true");
        request.Content = new StreamContent(fileStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Supabase campaign image upload failed. Status={Status}, Path={Path}, Body={Body}",
                (int)response.StatusCode,
                path,
                details);
            throw new InvalidOperationException($"Supabase upload failed: {(int)response.StatusCode} {details}");
        }

        return publicUrl;
    }
}
