using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class CancelCreditAccountUseCase : ICancelCreditAccountUseCase
{
    private readonly IAppDbContext _context;
    private readonly IGetCreditAccountByIdUseCase _getById;

    public CancelCreditAccountUseCase(IAppDbContext context, IGetCreditAccountByIdUseCase getById)
    {
        _context = context;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, Guid accountId)
    {
        var utcNow = DateTime.UtcNow;
        var account = await _context.CreditAccounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.BusinessId == businessId)
            ?? throw new NotFoundException("CreditAccount", "Cuenta de crédito no encontrada.");

        if (account.Status == (int)CreditAccountStatus.Cancelled)
            throw new InvalidOperationException("La cuenta ya está archivada.");

        var balance = CreditAccountBalanceResolver.Resolve(account);
        if (balance > 0)
            throw new InvalidOperationException("Solo se puede archivar una cuenta con saldo ₡0.");

        account.Status = (int)CreditAccountStatus.Cancelled;
        account.CancelledAt = utcNow;
        account.UpdatedAt = utcNow;
        account.CurrentBalanceCrc = 0;

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _getById.Execute(businessId, account.Id);
        return result ?? throw new InvalidOperationException("No se pudo leer la cuenta archivada.");
    }
}
