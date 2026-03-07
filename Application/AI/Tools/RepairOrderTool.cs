using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Infrastructure.Persistence;
using System.Text.Json;

namespace MiNegocioCR.Api.Application.AI.Tools
{
    public class RepairOrderTool : IAITool
    {
        private readonly AppDbContext _context;

        public string Name => "repair_order_search";

        public RepairOrderTool(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> ExecuteAsync(Guid businessId, string query)
        {
            query = query.ToLower();

            var rows = await _context.RepairOrders
                .Where(r =>
                    r.BusinessId == businessId &&
                    (
                        query.Contains(r.OrderNumber.ToString()) ||
                        query.Contains(r.CustomerPhone)
                    )
                )
                .Select(r => new
                {
                    r.OrderNumber,
                    r.CustomerName,
                    r.CustomerPhone,
                    r.DeviceDescription,
                    r.ProblemDescription,
                    r.Status,
                    r.CreatedAt
                })
                .ToListAsync();

            var orders = rows.Select(r => new
            {
                r.OrderNumber,
                r.CustomerName,
                r.CustomerPhone,
                r.DeviceDescription,
                r.ProblemDescription,
                Status = r.Status switch
                {
                    0 => "Pendiente",
                    1 => "En proceso",
                    2 => "Finalizada",
                    3 => "Entregada",
                    _ => "Desconocido"
                },
                r.CreatedAt
            }).ToList();

            return JsonSerializer.Serialize(orders);
        }
    }
}
