using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class SupabaseRepairOrderImageStorageService : IRepairOrderImageStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;

    public SupabaseRepairOrderImageStorageService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
        _serviceKey = configuration["Supabase:ServiceKey"] ?? string.Empty;
        _bucket = configuration["Supabase:StorageBucket"] ?? "business-assets";
    }

    public async Task<string> UploadAsync(
        Guid repairOrderId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl))
            throw new InvalidOperationException("Supabase config missing");
        if (string.IsNullOrWhiteSpace(_serviceKey))
            throw new InvalidOperationException("Supabase config missing");

        var ext = GetExtension(contentType);
        var objectName = $"{Guid.NewGuid():N}{ext}";
        var path = $"repair-orders/{repairOrderId}/{objectName}";
        var baseUrl = _supabaseUrl.TrimEnd('/');
        var uploadUrl = $"{baseUrl}/storage/v1/object/{_bucket}/{path}";
        var publicUrl = $"{baseUrl}/storage/v1/object/public/{_bucket}/{path}";

        var mediaType = NormalizeMediaType(contentType);

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
        request.Headers.Add("apikey", _serviceKey);
        request.Headers.Add("x-upsert", "true");
        request.Content = new StreamContent(fileStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Supabase upload failed: {(int)response.StatusCode} {details}");
        }

        return publicUrl;
    }

    public async Task DeleteByPublicUrlAsync(string publicImageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicImageUrl))
            return;

        var path = ExtractObjectPathFromPublicUrl(publicImageUrl);
        if (string.IsNullOrEmpty(path))
            return;

        if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_serviceKey))
            return;

        var baseUrl = _supabaseUrl.TrimEnd('/');
        var deleteUrl = $"{baseUrl}/storage/v1/object/{_bucket}/{path}";

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
        request.Headers.Add("apikey", _serviceKey);

        using var response = await client.SendAsync(request, cancellationToken);
    }

    private static string GetExtension(string contentType)
    {
        if (contentType.Contains("png", StringComparison.OrdinalIgnoreCase))
            return ".png";
        return ".jpg";
    }

    private static string NormalizeMediaType(string contentType)
    {
        if (contentType.Contains("png", StringComparison.OrdinalIgnoreCase))
            return "image/png";
        return "image/jpeg";
    }

    private string? ExtractObjectPathFromPublicUrl(string publicUrl)
    {
        var marker = $"/object/public/{_bucket}/";
        var idx = publicUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return null;
        return publicUrl[(idx + marker.Length)..];
    }
}
