using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Aplication.UseCases.RepairOrder
{
    public class GetRepairOrderByBusinessIdAndStatusUseCase : IGetRepairOrderByBusinessIdAndStatusUseCase
    {
        private readonly IAppDbContext _context;

        public GetRepairOrderByBusinessIdAndStatusUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> Execute(Guid businessId, RepairOrderStatus status)
        {
            var list = await _context.RepairOrders
                .Where(x => x.BusinessId == businessId && (RepairOrderStatus)x.Status == status)
                .OrderByDescending(x => x.OrderNumber)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.CustomerName,
                    x.CustomerPhone,
                    x.CustomerEmail,
                    x.DeviceDescription,
                    x.ProblemDescription,
                    Status = ((RepairOrderStatus)x.Status).ToString(),
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return list.Cast<object>().ToList();
        }
    }
}
