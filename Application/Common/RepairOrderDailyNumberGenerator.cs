using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Genera <c>OrderNumber</c> como YYYYMMDD + 3 dígitos (p. ej. 20260421005), consecutivo por negocio y fecha UTC.
/// </summary>
public static class RepairOrderDailyNumberGenerator
{
    public const int DaySequenceDigits = 3;
    public const int TotalLength = 8 + DaySequenceDigits; // 11

    public static string DatePrefix(DateTime utcDate) =>
        DateTime.SpecifyKind(utcDate, DateTimeKind.Utc).ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    /// <summary>Calcula el siguiente número (bloquea concurrencia del negocio fuera; usar dentro de transacción con mutex).</summary>
    public static async Task<string> GetNextForBusinessAndUtcDateAsync(
        IQueryable<RepairOrder> repairOrders,
        Guid businessId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var prefix = DatePrefix(utcNow);
        var existing = await repairOrders
            .AsNoTracking()
            .Where(r =>
                r.BusinessId == businessId
                && r.OrderNumber != null
                && r.OrderNumber.Length == TotalLength
                && r.OrderNumber.StartsWith(prefix))
            .Select(r => r.OrderNumber)
            .ToListAsync(cancellationToken);

        var next = 1;
        if (existing.Count > 0)
        {
            var last = existing.Max()!;
            if (last.Length == TotalLength
                && int.TryParse(last[^DaySequenceDigits..], NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            {
                next = n + 1;
            }
        }

        if (next > 999)
        {
            throw new InvalidOperationException(
                "Se alcanzó el máximo de 999 reparaciones para la fecha. Use el soporte de MiNegocioCR.");
        }

        return prefix + next.ToString("D" + DaySequenceDigits, CultureInfo.InvariantCulture);
    }
}
