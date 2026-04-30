using System.Globalization;

namespace MiNegocioCR.Api.API.Http;

internal static class QueryParamParsing
{
    /// <summary>Parses yyyy-MM-dd (recommended) or invariant date strings for filter ranges.</summary>
    public static DateTime? ParseUtcDayStart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();
        if (DateOnly.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
            return DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            return DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);

        return null;
    }

    public static int ParsePositiveInt(string? value, int defaultValue, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            return defaultValue;
        if (n <= 0) return defaultValue;
        return n > max ? max : n;
    }
}
