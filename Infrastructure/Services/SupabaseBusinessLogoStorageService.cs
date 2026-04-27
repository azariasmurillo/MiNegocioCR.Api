using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class SupabaseBusinessLogoStorageService : IBusinessLogoStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;

    public SupabaseBusinessLogoStorageService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
        _serviceKey = configuration["Supabase:ServiceKey"] ?? string.Empty;
        _bucket = configuration["Supabase:StorageBucket"] ?? "business-assets";
    }

    public async Task<string> UploadLogoAsync(
        Guid businessId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl))
            throw new Exception("Supabase config missing");
        if (string.IsNullOrWhiteSpace(_serviceKey))
            throw new Exception("Supabase config missing");

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
            throw new InvalidOperationException($"Supabase upload failed: {(int)response.StatusCode} {details}");
        }

        return publicUrl;
    }
}
