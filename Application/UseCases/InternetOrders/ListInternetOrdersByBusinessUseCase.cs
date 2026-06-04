using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.InternetOrders;

public class ListInternetOrdersByBusinessUseCase : IListInternetOrdersByBusinessUseCase
{
    private readonly IAppDbContext _context;

    public ListInternetOrdersByBusinessUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> Execute(Guid businessId, string? statusFilter, string? search)
    {
        var query = _context.InternetOrders
            .AsNoTracking()
            .Include(x => x.Contact)
            .Include(x => x.Lines)
            .Where(x => x.BusinessId == businessId);

        if (!string.IsNullOrWhiteSpace(statusFilter)
            && Enum.TryParse<InternetOrderStatus>(statusFilter.Trim(), true, out var status))
        {
            query = query.Where(x => x.Status == (int)status);
        }

        var term = (search ?? string.Empty).Trim().ToLowerInvariant();
        if (term.Length > 0)
        {
            query = query.Where(x =>
                x.OrderNumber.ToLower().Contains(term)
                || x.Contact.Name.ToLower().Contains(term)
                || x.Contact.Phone.ToLower().Contains(term)
                || (x.Contact.Email != null && x.Contact.Email.ToLower().Contains(term)));
        }

        var list = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return list.Select(InternetOrderProjection.MapSummary).Cast<object>().ToList();
    }
}
