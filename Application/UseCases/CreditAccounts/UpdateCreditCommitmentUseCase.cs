using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class UpdateCreditCommitmentUseCase : IUpdateCreditCommitmentUseCase
{
    private readonly IAppDbContext _context;
    private readonly IGetCreditAccountByIdUseCase _getById;

    public UpdateCreditCommitmentUseCase(IAppDbContext context, IGetCreditAccountByIdUseCase getById)
    {
        _context = context;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, Guid accountId, UpdateCreditCommitmentRequestDto request)
    {
        var utcNow = DateTime.UtcNow;
        var account = await _context.CreditAccounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.BusinessId == businessId)
            ?? throw new NotFoundException("CreditAccount", "Cuenta de crédito no encontrada.");

        var previousDate = account.PaymentCommitmentDate;
        var nextDate = CreditCommitmentDateNormalizer.ToStorageDate(request.PaymentCommitmentDate);
        var dateChanged = !CreditCommitmentDateNormalizer.SameCalendarDay(previousDate, nextDate);

        account.PaymentCommitmentDate = nextDate;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            account.Notes = request.Notes.Trim();
        account.UpdatedAt = utcNow;

        var db = (DbContext)_context;
        db.Entry(account).Property(a => a.PaymentCommitmentDate).IsModified = true;
        db.Entry(account).Property(a => a.UpdatedAt).IsModified = true;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            db.Entry(account).Property(a => a.Notes).IsModified = true;

        if (dateChanged)
        {
            var balance = CreditAccountBalanceResolver.Resolve(account);
            var renewalNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            _context.CreditTransactions.Add(new CreditTransaction
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CreditAccountId = account.Id,
                ContactId = account.ContactId,
                TransactionType = (int)CreditTransactionType.Renovacion,
                AmountCrc = 0,
                Description = $"Renovación compromiso: {FormatCommitmentDate(previousDate)} → {FormatCommitmentDate(nextDate)}",
                PreviousBalanceCrc = balance,
                NewBalanceCrc = balance,
                Notes = renewalNotes,
                CreatedAt = utcNow,
            });
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _getById.Execute(businessId, account.Id);
        return result ?? throw new InvalidOperationException("No se pudo leer la cuenta actualizada.");
    }

    private static string FormatCommitmentDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("dd/MM/yyyy") : "Sin fecha";
}
