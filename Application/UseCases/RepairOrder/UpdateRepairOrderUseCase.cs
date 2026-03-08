using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder
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
            if (request == null) throw new ArgumentNullException(nameof(request));
            var order = await _context.RepairOrders.FindAsync(id);

            if (order == null)
                throw new NotFoundException("RepairOrder", "Order not found");

            if ((RepairOrderStatus)order.Status == RepairOrderStatus.Delivered)
                throw new ArgumentException("Delivered orders cannot be modified.");

            order.CustomerName = request.CustomerName;
            order.CustomerPhone = PhoneSanitizer.Sanitize(request.CustomerPhone);
            order.CustomerEmail = request.CustomerEmail;
            order.DeviceDescription = request.DeviceDescription;
            order.ProblemDescription = request.ProblemDescription;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
