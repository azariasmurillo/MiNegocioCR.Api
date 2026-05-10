using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class SupabaseBusinessLogoStorageService : IBusinessLogoStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabaseBusinessLogoStorageService> _logger;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;

    public SupabaseBusinessLogoStorageService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SupabaseBusinessLogoStorageService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        var resolved = SupabaseStorageConfigurationReader.Read(configuration);
        _supabaseUrl = resolved.Url;
        _serviceKey = resolved.ServiceKey;
        _bucket = resolved.Bucket;
    }

    public async Task<string> UploadLogoAsync(
        Guid businessId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl))
            throw new InvalidOperationException(
                "Supabase config missing: set Supabase:Url or SUPABASE_URL (project URL, e.g. https://xxx.supabase.co).");
        if (string.IsNullOrWhiteSpace(_serviceKey))
            throw new InvalidOperationException(
                "Supabase config missing: set Supabase:ServiceKey or SUPABASE_SERVICE_ROLE_KEY (service_role, not anon).");

        var path = $"logos/{businessId}.png";
        var baseUrl = _supabaseUrl.TrimEnd('/');
        var uploadUrl = $"{baseUrl}/storage/v1/object/{_bucket}/{path}";
        var publicUrl = $"{baseUrl}/storage/v1/object/public/{_bucket}/{path}";

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
        request.Headers.Add("apikey", _serviceKey);
        request.Headers.Add("x-upsert", "true");
        request.Content = new StreamContent(fileStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Supabase logo upload failed. Status={Status}, Bucket={Bucket}, Path={Path}, Body={Body}",
                (int)response.StatusCode,
                _bucket,
                path,
                details);
            throw new InvalidOperationException($"Supabase upload failed: {(int)response.StatusCode} {details}");
        }

        _logger.LogInformation(
            "Supabase logo uploaded. Bucket={Bucket}, Path={Path}",
            _bucket,
            path);

        return publicUrl;
    }
}
