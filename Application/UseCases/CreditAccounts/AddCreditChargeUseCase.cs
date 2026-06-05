using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class AddCreditChargeUseCase : IAddCreditChargeUseCase
{
    private readonly IAppDbContext _context;
    private readonly IInventoryService _inventory;
    private readonly IGetCreditAccountByIdUseCase _getById;

    public AddCreditChargeUseCase(
        IAppDbContext context,
        IInventoryService inventory,
        IGetCreditAccountByIdUseCase getById)
    {
        _context = context;
        _inventory = inventory;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, Guid? accountId, CreateCreditChargeRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var (resolvedLines, totalCrc) = CreditChargeCalculator.ResolveLines(request.Lines);
        var utcNow = DateTime.UtcNow;

        using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync();

        CreditAccount account;
        if (accountId.HasValue)
        {
            account = await _context.CreditAccounts
                .Include(a => a.Contact)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == accountId.Value && a.BusinessId == businessId)
                ?? throw new NotFoundException("CreditAccount", "Cuenta de crédito no encontrada.");

            if (account.Status == (int)CreditAccountStatus.Cancelled)
                throw new InvalidOperationException("La cuenta está cancelada.");
        }
        else
        {
            var contact = await RepairOrderContactHelper.ResolveContactForCreateAsync(
                _context,
                businessId,
                request.ContactId,
                request.CustomerName ?? string.Empty,
                request.CustomerPhone,
                request.CustomerEmail);

            account = await _context.CreditAccounts
                .Include(a => a.Contact)
                .FirstOrDefaultAsync(a => a.BusinessId == businessId && a.ContactId == contact.Id);

            if (account == null)
            {
                var accountNumber = await CreditAccountDailyNumberGenerator.GetNextForBusinessAndUtcDateAsync(
                    _context.CreditAccounts,
                    businessId,
                    utcNow,
                    CancellationToken.None);

                account = new CreditAccount
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    ContactId = contact.Id,
                    AccountNumber = accountNumber,
                    Status = (int)CreditAccountStatus.Active,
                    CurrentBalanceCrc = 0,
                    TotalChargedCrc = 0,
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow,
                    Contact = contact,
                };
                _context.CreditAccounts.Add(account);
            }
        }

        if (request.PaymentCommitmentDate.HasValue)
            account.PaymentCommitmentDate = CreditCommitmentDateNormalizer.ToStorageDate(request.PaymentCommitmentDate);

        var previousBalance = CreditAccountBalanceResolver.Resolve(account);
        var txId = Guid.NewGuid();
        var reference = $"Credit:{txId:N}";

        foreach (var line in resolvedLines.Where(l => l.LineKind == CreditTransactionLineKind.Inventory))
        {
            await _inventory.DecreaseStockAsync(
                businessId,
                line.CatalogVariantId!.Value,
                line.Quantity,
                reference);
        }

        var entityLines = resolvedLines.Select(l => new CreditTransactionLine
        {
            Id = Guid.NewGuid(),
            CreditTransactionId = txId,
            SortOrder = l.SortOrder,
            LineKind = (int)l.LineKind,
            CatalogVariantId = l.CatalogVariantId,
            ConceptName = l.ConceptName,
            Quantity = l.Quantity,
            BaseUnitPriceCrc = l.BaseUnitPriceCrc,
            CreditMarkupPercent = l.CreditMarkupPercent,
            UnitPriceCrc = l.UnitPriceCrc,
            LineTotalCrc = l.LineTotalCrc,
        }).ToList();

        var description = string.Join(", ", entityLines.Select(l => l.ConceptName).Take(3));
        if (entityLines.Count > 3)
            description += $" (+{entityLines.Count - 3})";

        var creditTx = new CreditTransaction
        {
            Id = txId,
            BusinessId = businessId,
            CreditAccountId = account.Id,
            ContactId = account.ContactId,
            TransactionType = (int)CreditTransactionType.Credito,
            AmountCrc = totalCrc,
            Description = description,
            PreviousBalanceCrc = previousBalance,
            NewBalanceCrc = CreditChargeCalculator.RoundCrc(previousBalance + totalCrc),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = utcNow,
            Lines = entityLines,
        };

        account.CurrentBalanceCrc = creditTx.NewBalanceCrc;
        account.TotalChargedCrc = CreditChargeCalculator.RoundCrc(account.TotalChargedCrc + totalCrc);
        CreditAccountStatusHelper.SyncAfterBalanceChange(account, utcNow);

        _context.CreditTransactions.Add(creditTx);
        await _context.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync();

        var result = await _getById.Execute(businessId, account.Id);
        return result ?? throw new InvalidOperationException("No se pudo leer la cuenta actualizada.");
    }
}
