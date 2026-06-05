using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

public static class CreditAccountStatusHelper
{
    public static void SyncAfterBalanceChange(CreditAccount account, DateTime utcNow)
    {
        account.UpdatedAt = utcNow;
        if (account.Status == (int)CreditAccountStatus.Cancelled)
            return;

        if (account.CurrentBalanceCrc <= 0)
        {
            account.CurrentBalanceCrc = 0;
            account.Status = (int)CreditAccountStatus.Paid;
            account.PaidAt ??= utcNow;
            return;
        }

        account.Status = (int)CreditAccountStatus.Active;
        account.PaidAt = null;
    }

    public static bool IsOverdue(CreditAccount account, DateTime utcNow)
    {
        if (account.CurrentBalanceCrc <= 0 || account.PaymentCommitmentDate == null)
            return false;
        return account.PaymentCommitmentDate.Value.Date < utcNow.Date;
    }

    public static bool HasPartialPayments(IEnumerable<CreditTransaction> transactions) =>
        transactions.Any(t => t.TransactionType == (int)CreditTransactionType.Abono);
}
