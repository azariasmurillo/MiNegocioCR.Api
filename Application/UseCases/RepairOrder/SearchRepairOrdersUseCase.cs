using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class SearchRepairOrdersUseCase : ISearchRepairOrdersUseCase
{
    private readonly IAppDbContext _context;

    public SearchRepairOrdersUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> Execute(Guid businessId, string? query, DateTime? fromUtc, DateTime? toUtc)
    {
        IQueryable<Domain.Entities.RepairOrder> q = _context.RepairOrders
            .AsNoTracking()
            .Where(r => r.BusinessId == businessId);

        if (fromUtc.HasValue)
        {
            var f = fromUtc.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(fromUtc.Value, DateTimeKind.Utc)
                : fromUtc.Value.ToUniversalTime();
            q = q.Where(r => r.CreatedAt >= f);
        }

        if (toUtc.HasValue)
        {
            var t = toUtc.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(toUtc.Value, DateTimeKind.Utc)
                : toUtc.Value.ToUniversalTime();
            q = q.Where(r => r.CreatedAt <= t);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var t = query.Trim();
            var lower = t.ToLower();
            q = q.Where(r =>
                r.OrderNumber != null && r.OrderNumber.ToLower().Contains(lower)
                || (r.Contact.Name != null && r.Contact.Name.ToLower().Contains(lower))
                || (r.Contact.Phone != null && r.Contact.Phone.ToLower().Contains(lower))
                || (r.Contact.Email != null && r.Contact.Email.ToLower().Contains(lower)));
        }

        var list = await q
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.OrderNumber,
                r.ContactId,
                Contact = new
                {
                    r.Contact.Id,
                    Name = r.Contact.Name,
                    Phone = r.Contact.Phone,
                    Email = r.Contact.Email
                },
                r.DeviceDescription,
                r.ProblemDescription,
                Status = ((RepairOrderStatus)r.Status).ToString(),
                r.IsActive,
                r.CreatedAt,
                r.UpdatedAt,
                Items = r.Items
                    .OrderBy(i => i.Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.CatalogVariantId,
                        i.Description,
                        i.Quantity,
                        i.Price
                    })
            })
            .ToListAsync();

        return list.Cast<object>().ToList();
    }
}
