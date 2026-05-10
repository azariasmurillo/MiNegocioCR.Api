using Microsoft.Extensions.Configuration;

namespace MiNegocioCR.Api.Infrastructure.Services;

/// <summary>
/// Lee URL, service role key y bucket para Storage REST.
/// Acepta sección <c>Supabase:*</c> o variables planas típicas de Railway / Supabase.
/// </summary>
internal static class SupabaseStorageConfigurationReader
{
    public static SupabaseStorageResolved Read(IConfiguration configuration)
    {
        var url = FirstNonEmpty(
            configuration["Supabase:Url"],
            configuration["SUPABASE_URL"]);

        url = NormalizeSupabaseProjectUrl(url);

        var serviceKey = FirstNonEmpty(
            configuration["Supabase:ServiceKey"],
            configuration["SUPABASE_SERVICE_ROLE_KEY"],
            configuration["SUPABASE_SERVICE_KEY"]);

        var bucket = FirstNonEmpty(
            configuration["Supabase:StorageBucket"],
            configuration["SUPABASE_STORAGE_BUCKET"])
            ?? "business-assets";

        return new SupabaseStorageResolved(url ?? string.Empty, serviceKey ?? string.Empty, bucket);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                return v.Trim();
        }

        return null;
    }

    /// <summary>
    /// Acepta <c>https://xxx.supabase.co</c> o una URL pegada por error que ya incluye <c>/storage/v1</c>.
    /// </summary>
    private static string? NormalizeSupabaseProjectUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        url = url.Trim().TrimEnd('/');
        const string storageSuffix = "/storage/v1";
        if (url.EndsWith(storageSuffix, StringComparison.OrdinalIgnoreCase))
            url = url[..^storageSuffix.Length].TrimEnd('/');

        return url;
    }
}

internal readonly record struct SupabaseStorageResolved(string Url, string ServiceKey, string Bucket);
