using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class GetCreditDashboardSummaryUseCase : IGetCreditDashboardSummaryUseCase
{
    private readonly IAppDbContext _context;

    public GetCreditDashboardSummaryUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<object> Execute(Guid businessId)
    {
        var utcNow = DateTime.UtcNow;
        var today = utcNow.Date;
        var monthStart = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var accounts = await _context.CreditAccounts
            .AsNoTracking()
            .Include(a => a.Contact)
            .Include(a => a.Transactions)
            .Where(a => a.BusinessId == businessId && a.Status != (int)CreditAccountStatus.Cancelled)
            .ToListAsync();

        var active = accounts.Where(a => CreditAccountBalanceResolver.Resolve(a) > 0).ToList();
        var pendingTotal = active.Sum(a => CreditAccountBalanceResolver.Resolve(a));
        var overdueCount = active.Count(a =>
            a.PaymentCommitmentDate.HasValue && a.PaymentCommitmentDate.Value.Date < today);

        var paymentsMonth = await _context.CreditTransactions
            .AsNoTracking()
            .Where(t =>
                t.BusinessId == businessId
                && t.TransactionType == (int)CreditTransactionType.Abono
                && t.CreatedAt >= monthStart)
            .SumAsync(t => t.AmountCrc);

        var paidCount = accounts.Count(a => CreditAccountBalanceResolver.Resolve(a) <= 0);

        var topDebtors = active
            .OrderByDescending(a => CreditAccountBalanceResolver.Resolve(a))
            .Take(10)
            .Select(a => new
            {
                a.Id,
                a.AccountNumber,
                CustomerName = a.Contact?.Name,
                BalanceCrc = CreditAccountBalanceResolver.Resolve(a),
            })
            .ToList();

        return new
        {
            ActiveAccounts = active.Count,
            PendingTotalCrc = pendingTotal,
            OverdueAccounts = overdueCount,
            PaymentsMonthCrc = paymentsMonth,
            PaidAccounts = paidCount,
            TopDebtors = topDebtors,
        };
    }
}
