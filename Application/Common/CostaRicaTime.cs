namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Fechas de negocio en zona horaria Costa Rica (America/Costa_Rica).
/// Los timestamps en BD están en UTC; los filtros "hoy" y rangos del dashboard usan este helper.
/// </summary>
public static class CostaRicaTime
{
    public const string IanaId = "America/Costa_Rica";
    public const string WindowsId = "Central America Standard Time";

    private static readonly TimeZoneInfo Tz = ResolveTimeZone();

    public static TimeZoneInfo Zone => Tz;

    public static DateOnly Today => ToLocalDate(DateTime.UtcNow);

    /// <summary>Inicio del día calendario en CR (00:00), como instante UTC para comparar con <c>CreatedAt</c>.</summary>
    public static DateTime ToUtcStartOfDay(DateOnly day)
    {
        var local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, Tz);
    }

    /// <summary>Límite exclusivo del rango: inicio del día siguiente en CR, en UTC.</summary>
    public static DateTime ToUtcEndExclusive(DateOnly lastInclusiveDay) =>
        ToUtcStartOfDay(lastInclusiveDay.AddDays(1));

    public static DateOnly ToLocalDate(DateTime utc)
    {
        var normalized = utc.Kind switch
        {
            DateTimeKind.Utc => utc,
            DateTimeKind.Local => utc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utc, DateTimeKind.Utc)
        };
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(normalized, Tz));
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(IanaId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(WindowsId);
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(WindowsId);
        }
    }
}
