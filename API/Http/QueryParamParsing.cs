using System.Globalization;
using MiNegocioCR.Api.Application.Common;

namespace MiNegocioCR.Api.API.Http;

internal static class QueryParamParsing
{
    /// <summary>
    /// Rango inclusivo de fechas calendario en Costa Rica → instantes UTC para filtrar <c>CreatedAt</c>.
    /// Acepta <c>yyyy-MM-dd</c> o ISO; solo se usa la parte de fecha.
    /// </summary>
    public static (DateTime? FromUtcInclusive, DateTime? ToUtcExclusive) ParseCostaRicaDateRange(
        string? from,
        string? to)
    {
        DateTime? fromUtc = null;
        DateTime? toExclusive = null;

        if (TryParseCalendarDate(from, out var fromDay))
            fromUtc = CostaRicaTime.ToUtcStartOfDay(fromDay);

        if (TryParseCalendarDate(to, out var toDay))
            toExclusive = CostaRicaTime.ToUtcEndExclusive(toDay);

        return (fromUtc, toExclusive);
    }

    /// <summary>Compatibilidad: inicio del día en CR (UTC). Preferir <see cref="ParseCostaRicaDateRange"/>.</summary>
    public static DateTime? ParseUtcDayStart(string? value)
    {
        if (!TryParseCalendarDate(value, out var day))
            return null;
        return CostaRicaTime.ToUtcStartOfDay(day);
    }

    public static int ParsePositiveInt(string? value, int defaultValue, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            return defaultValue;
        if (n <= 0) return defaultValue;
        return n > max ? max : n;
    }

    private static bool TryParseCalendarDate(string? value, out DateOnly day)
    {
        day = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (trimmed.Length >= 10 && DateOnly.TryParse(trimmed.AsSpan(0, 10), CultureInfo.InvariantCulture, out day))
            return true;

        if (DateOnly.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
            return true;

        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
        {
            day = DateOnly.FromDateTime(dt);
            return true;
        }

        return false;
    }
}
