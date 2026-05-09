namespace MiNegocioCR.Api.Application.Configuration;

/// <summary>URLs públicas para enlaces en emails (ej. recuperación de contraseña).</summary>
public class AppOptions
{
    public const string SectionName = "App";

    /// <summary>Base URL del frontend (sin slash final). Ej: https://app.minegociocr.com</summary>
    public string PublicUrl { get; set; } = "http://localhost:4200";
}
