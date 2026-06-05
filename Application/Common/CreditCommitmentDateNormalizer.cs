namespace MiNegocioCR.Api.Application.Common;

public static class CreditCommitmentDateNormalizer
{
    /// <summary>Guarda solo el día calendario a mediodía UTC (evita desfases CR ↔ UTC).</summary>
    public static DateTime? ToStorageDate(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        var d = value.Value;
        if (d.Kind == DateTimeKind.Local)
            d = d.ToUniversalTime();
        else if (d.Kind == DateTimeKind.Unspecified)
            d = DateTime.SpecifyKind(d, DateTimeKind.Utc);

        return new DateTime(d.Year, d.Month, d.Day, 12, 0, 0, DateTimeKind.Utc);
    }

    public static bool SameCalendarDay(DateTime? left, DateTime? right)
    {
        var a = ToStorageDate(left);
        var b = ToStorageDate(right);
        if (!a.HasValue && !b.HasValue)
            return true;
        if (!a.HasValue || !b.HasValue)
            return false;
        return a.Value.Date == b.Value.Date;
    }
}
