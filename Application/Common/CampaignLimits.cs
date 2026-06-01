namespace MiNegocioCR.Api.Application.Common;

public static class CampaignLimits
{
    /// <summary>Cupo diario global de campañas (todos los tenants).</summary>
    public const int PlatformDailyLimit = 495;

    /// <summary>Intervalo entre envíos de campaña en la cola global (segundos).</summary>
    public const int QueueSendIntervalSeconds = 60;

    public const int DefaultInactiveDays = 60;
    public const int DefaultQuietDays = 60;

    // Compatibilidad con preview/DTOs previos
    public const int DefaultDailyLimit = PlatformDailyLimit;
    public const int MaxDailyLimit = PlatformDailyLimit;
    public const int SendIntervalSeconds = QueueSendIntervalSeconds;
}
