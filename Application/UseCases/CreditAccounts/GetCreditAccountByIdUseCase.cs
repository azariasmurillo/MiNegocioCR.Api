using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class GetCreditAccountByIdUseCase : IGetCreditAccountByIdUseCase
{
    private readonly IAppDbContext _context;

    public GetCreditAccountByIdUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<object?> Execute(Guid businessId, Guid accountId)
    {
        var account = await _context.CreditAccounts
            .AsNoTracking()
            .Include(a => a.Contact)
            .Include(a => a.Transactions)
                .ThenInclude(t => t.Lines)
            .Include(a => a.Communications)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.BusinessId == businessId);

        return account == null ? null : CreditAccountProjection.ToDetail(account, DateTime.UtcNow);
    }
}
