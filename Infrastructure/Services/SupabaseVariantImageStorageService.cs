using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class SupabaseVariantImageStorageService : IVariantImageStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<SupabaseVariantImageStorageService> _logger;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;

    public SupabaseVariantImageStorageService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SupabaseVariantImageStorageService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = configuration;
        _logger = logger;
        var resolved = SupabaseStorageConfigurationReader.Read(configuration);
        _supabaseUrl = resolved.Url;
        _serviceKey = resolved.ServiceKey;
        _bucket = resolved.Bucket;
    }

    public async Task<string> UploadAsync(
        Guid catalogVariantId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"Bucket: {_config["Supabase:StorageBucket"]}");
            Console.WriteLine($"URL: {_config["Supabase:Url"]}");

            if (string.IsNullOrWhiteSpace(_supabaseUrl))
                throw new InvalidOperationException(
                    "Supabase config missing: set Supabase:Url or SUPABASE_URL (project URL, e.g. https://xxx.supabase.co).");
            if (string.IsNullOrWhiteSpace(_serviceKey))
                throw new InvalidOperationException(
                    "Supabase config missing: set Supabase:ServiceKey or SUPABASE_SERVICE_ROLE_KEY (service_role, not anon).");

            var ext = GetExtension(contentType);
            var objectName = $"{Guid.NewGuid():N}{ext}";
            var path = $"variant/{catalogVariantId}/{objectName}";
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
                _logger.LogWarning(
                    "Supabase variant image upload failed. Status={Status}, Bucket={Bucket}, Path={Path}, Body={Body}",
                    (int)response.StatusCode,
                    _bucket,
                    path,
                    details);
                throw new InvalidOperationException($"Supabase upload failed: {(int)response.StatusCode} {details}");
            }

            _logger.LogInformation(
                "Supabase variant image uploaded. Bucket={Bucket}, Path={Path}",
                _bucket,
                path);

            return publicUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 ERROR SUPABASE:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
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
