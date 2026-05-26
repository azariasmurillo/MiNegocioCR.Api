using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder
{
    public class GetRepairOrderByIdUseCase : IGetRepairOrderByIdUseCase
    {
        private readonly IAppDbContext _context;

        public GetRepairOrderByIdUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<object?> Execute(Guid businessId, Guid id)
        {
            var order = await _context.RepairOrders
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Contact)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CatalogVariant)
                        .ThenInclude(v => v!.CatalogItem)
                .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.Id == id);

            if (order == null)
                return null;

            return new
            {
                order.Id,
                order.OrderNumber,
                order.ContactId,
                Contact = new
                {
                    order.Contact.Id,
                    Name = order.Contact.Name,
                    Phone = order.Contact.Phone,
                    Email = order.Contact.Email
                },
                order.ProblemDescription,
                order.DeviceType,
                order.DeviceTypeOther,
                order.Brand,
                order.Model,
                order.SerialNumber,
                order.AccessoriesIncluded,
                order.OperatingSystem,
                order.Password,
                order.IsDiagnosticPaid,
                order.DiscountPercent,
                Status = ((RepairOrderStatus)order.Status).ToString(),
                order.CreatedAt,
                order.UpdatedAt,
                Items = order.Items
                    .OrderBy(i => i.Id)
                    .Select(RepairOrderItemProjection.Map)
                    .ToList()
            };
        }
    }
}
