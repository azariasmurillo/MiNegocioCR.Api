namespace MiNegocioCR.Api.Application.Common;

public static class CampaignImageLimits
{
    /// <summary>Máximo que el usuario puede subir desde el navegador.</summary>
    public const int MaxUploadBytes = 5 * 1024 * 1024;

    public const string MaxUploadLabel = "5 MB";

    /// <summary>Ancho máximo en el HTML del correo (px).</summary>
    public const int MaxDisplayWidth = 640;

    /// <summary>Objetivo tras optimizar en servidor (~400 KB).</summary>
    public const int TargetOptimizedBytes = 400 * 1024;
}
