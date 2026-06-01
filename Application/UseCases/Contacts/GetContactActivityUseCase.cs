using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class GetContactActivityUseCase : IGetContactActivityUseCase
{
    private readonly IAppDbContext _context;

    public GetContactActivityUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContactActivityResultDto?> Execute(Guid businessId, Guid contactId, int take = 15)
    {
        if (take < 1)
            take = 15;
        if (take > 50)
            take = 50;

        var contact = await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contactId && c.BusinessId == businessId && !c.IsDeleted);

        if (contact == null)
            return null;

        var saleItems = await _context.Sales
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId && s.ContactId == contactId)
            .OrderByDescending(s => s.SaleDate)
            .Take(take)
            .Select(s => new ContactActivityItemDto
            {
                Type = "Sale",
                OccurredAt = s.SaleDate,
                Amount = s.TotalOrden,
                Label = s.InvoiceNumber
            })
            .ToListAsync();

        var paymentItems = await _context.Payments
            .AsNoTracking()
            .Where(p => p.BusinessId == businessId && p.Amount > 0)
            .Join(
                _context.RepairOrders.AsNoTracking().Where(r => r.ContactId == contactId),
                p => p.RepairOrderId,
                r => r.Id,
                (p, r) => new ContactActivityItemDto
                {
                    Type = "RepairPayment",
                    OccurredAt = p.CreatedAt,
                    Amount = p.Amount,
                    Label = r.OrderNumber
                })
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .ToListAsync();

        var items = saleItems
            .Concat(paymentItems)
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .ToList();

        return new ContactActivityResultDto
        {
            ContactId = contact.Id,
            Name = contact.Name,
            LastActivityAt = contact.LastActivityAt,
            Items = items
        };
    }
}
