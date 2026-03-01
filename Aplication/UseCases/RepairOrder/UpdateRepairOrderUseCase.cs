using MiNegocioCR.Api.Aplication.DTOs;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Aplication.UseCases.RepairOrder
{
    public class UpdateRepairOrderUseCase : IUpdateRepairOrderUseCase
    {
        private readonly IAppDbContext _context;

        public UpdateRepairOrderUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Execute(Guid id, UpdateRepairOrderRequestDto request)
        {
            var order = await _context.RepairOrders.FindAsync(id);

            if (order == null)
                throw new Exception("Order not found");

            if ((RepairOrderStatus)order.Status == RepairOrderStatus.Delivered)
                throw new Exception("Delivered orders cannot be modified.");

            order.CustomerName = request.CustomerName;
            order.CustomerPhone = request.CustomerPhone;
            order.CustomerEmail = request.CustomerEmail;
            order.DeviceDescription = request.DeviceDescription;
            order.ProblemDescription = request.ProblemDescription;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
