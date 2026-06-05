using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class RegisterCreditPaymentUseCase : IRegisterCreditPaymentUseCase
{
    private readonly IAppDbContext _context;
    private readonly IGetCreditAccountByIdUseCase _getById;

    public RegisterCreditPaymentUseCase(IAppDbContext context, IGetCreditAccountByIdUseCase getById)
    {
        _context = context;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, Guid accountId, RegisterCreditPaymentRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var utcNow = DateTime.UtcNow;
        var account = await _context.CreditAccounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.BusinessId == businessId)
            ?? throw new NotFoundException("CreditAccount", "Cuenta de crédito no encontrada.");

        if (account.Status == (int)CreditAccountStatus.Cancelled)
            throw new InvalidOperationException("La cuenta está cancelada.");

        var currentBalance = CreditAccountBalanceResolver.Resolve(account);
        if (currentBalance <= 0)
            throw new InvalidOperationException("La cuenta no tiene saldo pendiente.");

        var (applied, change, newBalance) = CreditChargeCalculator.ApplyPayment(
            currentBalance,
            request.AmountCrc);

        var previousBalance = currentBalance;
        var paymentTx = new CreditTransaction
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CreditAccountId = account.Id,
            ContactId = account.ContactId,
            TransactionType = (int)CreditTransactionType.Abono,
            AmountCrc = CreditChargeCalculator.RoundCrc(request.AmountCrc),
            AppliedToBalanceCrc = applied,
            ChangeGivenCrc = change > 0 ? change : null,
            Description = change > 0 ? $"Abono (vuelto ₡{change:N0})" : "Abono",
            PreviousBalanceCrc = previousBalance,
            NewBalanceCrc = newBalance,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = request.PaidAt ?? utcNow,
        };

        account.CurrentBalanceCrc = newBalance;
        CreditAccountStatusHelper.SyncAfterBalanceChange(account, utcNow);

        _context.CreditTransactions.Add(paymentTx);
        ((DbContext)_context).Entry(account).Property(a => a.CurrentBalanceCrc).IsModified = true;
        ((DbContext)_context).Entry(account).Property(a => a.Status).IsModified = true;
        ((DbContext)_context).Entry(account).Property(a => a.UpdatedAt).IsModified = true;
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _getById.Execute(businessId, account.Id);
        return result ?? throw new InvalidOperationException("No se pudo leer la cuenta actualizada.");
    }
}
