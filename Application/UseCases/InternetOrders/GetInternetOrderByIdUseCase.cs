using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;

namespace MiNegocioCR.Api.Application.UseCases.InternetOrders;

public class GetInternetOrderByIdUseCase : IGetInternetOrderByIdUseCase
{
    private readonly IAppDbContext _context;

    public GetInternetOrderByIdUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<object?> Execute(Guid businessId, Guid id)
    {
        var order = await _context.InternetOrders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Contact)
            .Include(x => x.Lines)
            .Include(x => x.Advances)
            .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.Id == id);

        if (order == null)
            return null;

        return InternetOrderProjection.MapDetail(order, includeExchangeRate: true);
    }
}
