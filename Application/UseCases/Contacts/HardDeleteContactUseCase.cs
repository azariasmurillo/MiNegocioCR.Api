using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class HardDeleteContactUseCase : IHardDeleteContactUseCase
{
    private readonly IAppDbContext _context;

    public HardDeleteContactUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Execute(Guid businessId, Guid id)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);

        if (contact == null)
            throw new NotFoundException("Contact", "Contact not found.");

        var hasSales = await _context.Sales
            .AnyAsync(s => s.BusinessId == businessId && s.ContactId == id);
        if (hasSales)
            throw new InvalidOperationException("Contact cannot be hard-deleted because it is referenced by Sales.");

        var hasRepairOrders = await _context.RepairOrders
            .AnyAsync(r => r.BusinessId == businessId && r.ContactId == id);
        if (hasRepairOrders)
            throw new InvalidOperationException("Contact cannot be hard-deleted because it is referenced by RepairOrders.");

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
