using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Aplication.UseCases.RepairOrder
{
    public class GetRepairOrderByIdUseCase : IGetRepairOrderByIdUseCase
    {
        private readonly IAppDbContext _context;

        public GetRepairOrderByIdUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<object?> Execute(Guid id)
        {
            return await _context.RepairOrders
                .Where(x => x.Id == id)
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
                .FirstOrDefaultAsync();
        }
    }
}
