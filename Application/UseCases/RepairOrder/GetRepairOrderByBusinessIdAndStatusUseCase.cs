using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder
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
                    x.ContactId,
                    Contact = new
                    {
                        x.Contact.Id,
                        Name = x.Contact.Name,
                        Phone = x.Contact.Phone,
                        Email = x.Contact.Email
                    },
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
