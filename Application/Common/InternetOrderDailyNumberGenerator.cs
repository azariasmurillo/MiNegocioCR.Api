using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class InternetOrderDailyNumberGenerator
{
    public const int DaySequenceDigits = RepairOrderDailyNumberGenerator.DaySequenceDigits;
    public const int TotalLength = RepairOrderDailyNumberGenerator.TotalLength;

    public static async Task<string> GetNextForBusinessAndUtcDateAsync(
        IQueryable<InternetOrder> orders,
        Guid businessId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var prefix = RepairOrderDailyNumberGenerator.DatePrefix(utcNow);
        var existing = await orders
            .AsNoTracking()
            .Where(r =>
                r.BusinessId == businessId
                && r.OrderNumber.Length == TotalLength
                && r.OrderNumber.StartsWith(prefix))
            .Select(r => r.OrderNumber)
            .ToListAsync(cancellationToken);

        var next = 1;
        if (existing.Count > 0)
        {
            var last = existing.Max();
            if (last.Length == TotalLength
                && int.TryParse(last[^DaySequenceDigits..], NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            {
                next = n + 1;
            }
        }

        if (next > 999)
        {
            throw new InvalidOperationException(
                "Se alcanzó el máximo de 999 pedidos internet para la fecha. Contactá soporte MiNegocioCR.");
        }

        return prefix + next.ToString("D" + DaySequenceDigits, CultureInfo.InvariantCulture);
    }
}
