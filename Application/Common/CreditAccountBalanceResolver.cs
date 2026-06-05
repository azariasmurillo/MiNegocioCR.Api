using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Saldo efectivo recalculado desde el historial (cargos/abonos), no solo el último snapshot.
/// Evita errores cuando dos movimientos comparten timestamp o leen un saldo viejo.
/// </summary>
public static class CreditAccountBalanceResolver
{
    public static decimal Resolve(CreditAccount account)
    {
        var txs = account.Transactions;
        if (txs != null && txs.Count > 0)
            return ReplayFromTransactions(txs);

        return account.CurrentBalanceCrc;
    }

    public static decimal ReplayFromTransactions(IEnumerable<CreditTransaction> transactions)
    {
        decimal balance = 0;
        foreach (var tx in OrderForReplay(transactions))
            balance = ApplyTransaction(balance, tx);

        return CreditChargeCalculator.RoundCrc(balance);
    }

    internal static IEnumerable<CreditTransaction> OrderForReplay(IEnumerable<CreditTransaction> transactions) =>
        transactions
            .OrderBy(t => t.CreatedAt)
            .ThenBy(BalanceKindOrder)
            .ThenBy(t => t.Id);

    internal static int BalanceKindOrder(CreditTransaction tx) =>
        (CreditTransactionType)tx.TransactionType switch
        {
            CreditTransactionType.Credito => 0,
            CreditTransactionType.Abono => 1,
            _ => 2,
        };

    private static decimal ApplyTransaction(decimal balance, CreditTransaction tx)
    {
        switch ((CreditTransactionType)tx.TransactionType)
        {
            case CreditTransactionType.Credito:
                return CreditChargeCalculator.RoundCrc(balance + tx.AmountCrc);
            case CreditTransactionType.Abono:
                return CreditChargeCalculator.ApplyPayment(balance, tx.AmountCrc).NewBalance;
            default:
                return balance;
        }
    }
}
