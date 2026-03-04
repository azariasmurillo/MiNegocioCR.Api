
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Aplication.UseCases.RepairOrder
{
    public class GetRepairOrdersByBusinessUseCase : IGetRepairOrdersByBusinessUseCase
    {
        private readonly IAppDbContext _context;

        public GetRepairOrdersByBusinessUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> Execute(Guid businessId)
        {
            var list = await _context.RepairOrders
                .Where(x => x.BusinessId == businessId)
                .OrderByDescending(x => x.OrderNumber)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.CustomerName,
                    Status = ((RepairOrderStatus)x.Status).ToString(),
                    x.CreatedAt
                })
                .ToListAsync();

            return list.Cast<object>().ToList();
        }
    }
}
