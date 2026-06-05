using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

public static class CreditAccountProjection
{
    public static object ToSummary(CreditAccount account, DateTime utcNow)
    {
        var status = (CreditAccountStatus)account.Status;
        var balance = CreditAccountBalanceResolver.Resolve(account);
        var hasPayments = account.Transactions?.Any(t => t.TransactionType == (int)CreditTransactionType.Abono) ?? false;
        return new
        {
            account.Id,
            account.BusinessId,
            account.ContactId,
            account.AccountNumber,
            Status = balance <= 0 && status != CreditAccountStatus.Cancelled
                ? CreditAccountStatus.Paid.ToString()
                : status.ToString(),
            CurrentBalanceCrc = balance,
            account.TotalChargedCrc,
            account.PaymentCommitmentDate,
            account.Notes,
            account.CreatedAt,
            account.UpdatedAt,
            account.PaidAt,
            CustomerName = account.Contact?.Name,
            CustomerPhone = account.Contact?.Phone,
            CustomerEmail = account.Contact?.Email,
            IsPartial = balance > 0 && hasPayments,
            IsOverdue = balance > 0 && CreditAccountStatusHelper.IsOverdue(account, utcNow),
        };
    }

    public static object ToDetail(CreditAccount account, DateTime utcNow, int transactionLimit = 100)
    {
        var balance = CreditAccountBalanceResolver.Resolve(account);
        var txs = (account.Transactions ?? Array.Empty<CreditTransaction>())
            .OrderByDescending(t => t.CreatedAt)
            .Take(transactionLimit)
            .Select(ToTransaction)
            .ToList();

        var communications = (account.Communications ?? Array.Empty<CreditCommunication>())
            .OrderByDescending(c => c.CreatedAt)
            .Take(100)
            .Select(ToCommunication)
            .ToList();

        return new
        {
            account.Id,
            account.BusinessId,
            account.ContactId,
            account.AccountNumber,
            Status = balance <= 0 && account.Status != (int)CreditAccountStatus.Cancelled
                ? CreditAccountStatus.Paid.ToString()
                : ((CreditAccountStatus)account.Status).ToString(),
            CurrentBalanceCrc = balance,
            account.TotalChargedCrc,
            account.PaymentCommitmentDate,
            account.Notes,
            account.CreatedAt,
            account.UpdatedAt,
            account.PaidAt,
            CustomerName = account.Contact?.Name,
            CustomerPhone = account.Contact?.Phone,
            CustomerEmail = account.Contact?.Email,
            IsPartial = balance > 0
                && (account.Transactions?.Any(t => t.TransactionType == (int)CreditTransactionType.Abono) ?? false),
            IsOverdue = balance > 0 && CreditAccountStatusHelper.IsOverdue(account, utcNow),
            Transactions = txs,
            Communications = communications,
        };
    }

    private static object ToCommunication(CreditCommunication comm) => new
    {
        comm.Id,
        Type = ((CreditCommunicationType)comm.CommunicationType).ToString(),
        comm.Notes,
        comm.CreatedAt,
    };

    private static object ToTransaction(CreditTransaction tx)
    {
        return new
        {
            tx.Id,
            Type = ((CreditTransactionType)tx.TransactionType).ToString(),
            tx.AmountCrc,
            tx.AppliedToBalanceCrc,
            tx.ChangeGivenCrc,
            tx.Description,
            tx.PreviousBalanceCrc,
            tx.NewBalanceCrc,
            tx.Notes,
            tx.CreatedAt,
            Lines = (tx.Lines ?? Array.Empty<CreditTransactionLine>())
                .OrderBy(l => l.SortOrder)
                .Select(l => new
                {
                    l.Id,
                    l.SortOrder,
                    LineKind = ((CreditTransactionLineKind)l.LineKind).ToString(),
                    l.CatalogVariantId,
                    l.ConceptName,
                    l.Quantity,
                    l.BaseUnitPriceCrc,
                    l.CreditMarkupPercent,
                    l.UnitPriceCrc,
                    l.LineTotalCrc,
                }),
        };
    }
}
