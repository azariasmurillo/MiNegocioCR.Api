using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class CreditAccountDailyNumberGenerator
{
    public const string Prefix = "CR";
    public const int DaySequenceDigits = RepairOrderDailyNumberGenerator.DaySequenceDigits;
    public const int TotalLength = 13; // CR + yyyyMMdd + 3 dígitos

    public static async Task<string> GetNextForBusinessAndUtcDateAsync(
        IQueryable<CreditAccount> accounts,
        Guid businessId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var datePrefix = RepairOrderDailyNumberGenerator.DatePrefix(utcNow);
        var fullPrefix = Prefix + datePrefix;
        var existing = await accounts
            .AsNoTracking()
            .Where(a =>
                a.BusinessId == businessId
                && a.AccountNumber.Length == TotalLength
                && a.AccountNumber.StartsWith(fullPrefix))
            .Select(a => a.AccountNumber)
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
                "Se alcanzó el máximo de 999 cuentas de crédito para la fecha. Contactá soporte MiNegocioCR.");
        }

        return fullPrefix + next.ToString("D" + DaySequenceDigits, CultureInfo.InvariantCulture);
    }
}
