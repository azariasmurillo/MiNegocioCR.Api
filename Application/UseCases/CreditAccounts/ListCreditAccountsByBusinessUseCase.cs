using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class ListCreditAccountsByBusinessUseCase : IListCreditAccountsByBusinessUseCase
{
    private readonly IAppDbContext _context;

    public ListCreditAccountsByBusinessUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<object> Execute(Guid businessId, string? filter, string? search)
    {
        var utcNow = DateTime.UtcNow;
        var query = _context.CreditAccounts
            .AsNoTracking()
            .Include(a => a.Contact)
            .Include(a => a.Transactions)
            .Where(a => a.BusinessId == businessId);

        var f = (filter ?? string.Empty).Trim().ToLowerInvariant();
        if (f is "active" or "activos" or "activo")
        {
            query = query.Where(a =>
                a.Status != (int)CreditAccountStatus.Cancelled && a.CurrentBalanceCrc > 0);
        }
        else if (f is "paid" or "pagados" or "pagado")
        {
            query = query.Where(a =>
                a.Status != (int)CreditAccountStatus.Cancelled && a.CurrentBalanceCrc <= 0);
        }
        else if (f is "overdue" or "vencidos" or "vencido")
        {
            var today = utcNow.Date;
            query = query.Where(a =>
                a.Status != (int)CreditAccountStatus.Cancelled
                && a.CurrentBalanceCrc > 0
                && a.PaymentCommitmentDate != null
                && a.PaymentCommitmentDate.Value.Date < today);
        }
        else if (f is "archived" or "archivadas" or "archivada" or "cancelled" or "cancelado" or "cancelados")
        {
            query = query.Where(a => a.Status == (int)CreditAccountStatus.Cancelled);
        }

        var term = (search ?? string.Empty).Trim();
        if (term.Length > 0)
        {
            var like = $"%{term}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.AccountNumber, like)
                || EF.Functions.ILike(a.Contact.Name, like)
                || EF.Functions.ILike(a.Contact.Phone, like)
                || (a.Contact.Email != null && EF.Functions.ILike(a.Contact.Email, like)));
        }

        var rows = await query
            .OrderByDescending(a => a.UpdatedAt)
            .Take(500)
            .ToListAsync();

        return rows.Select(a => CreditAccountProjection.ToSummary(a, utcNow)).ToList();
    }
}
