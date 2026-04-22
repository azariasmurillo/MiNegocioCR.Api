
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder
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
                    x.ContactId,
                    Contact = new
                    {
                        x.Contact.Id,
                        Name = x.Contact.Name,
                        Phone = x.Contact.Phone,
                        Email = x.Contact.Email
                    },
                    Status = ((RepairOrderStatus)x.Status).ToString(),
                    x.CreatedAt
                })
                .ToListAsync();

            return list.Cast<object>().ToList();
        }
    }
}
