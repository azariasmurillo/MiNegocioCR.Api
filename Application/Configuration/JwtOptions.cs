namespace MiNegocioCR.Api.Application.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 120;

    // Compatibilidad temporal con configuración anterior.
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationHours { get; set; } = 0;

    public string ResolveSigningKey()
        => !string.IsNullOrWhiteSpace(Key) ? Key : SigningKey;

    public int ResolveExpirationMinutes()
        => ExpiresInMinutes > 0 ? ExpiresInMinutes : Math.Max(1, ExpirationHours * 60);
}
