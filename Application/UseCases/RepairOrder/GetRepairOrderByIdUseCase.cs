using Microsoft.EntityFrameworkCore;
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
            return await _context.RepairOrders
                .AsNoTracking()
                .Where(x => x.BusinessId == businessId && x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.ContactId,
                    Contact = new
                    {
                        x.Contact.Id,
                        Name = x.Contact.Name,
                        Phone = x.Contact.Phone,
                        Email = x.Contact.Email
                    },
                    x.ProblemDescription,
                    x.DeviceType,
                    x.DeviceTypeOther,
                    x.Brand,
                    x.Model,
                    x.SerialNumber,
                    x.AccessoriesIncluded,
                    x.OperatingSystem,
                    x.Password,
                    x.IsDiagnosticPaid,
                    x.DiscountPercent,
                    Status = ((RepairOrderStatus)x.Status).ToString(),
                    x.CreatedAt,
                    x.UpdatedAt,
                    Items = x.Items
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
                .FirstOrDefaultAsync();
        }
    }
}
