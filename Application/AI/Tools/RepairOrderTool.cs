using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.Common;
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

        public async Task<ToolResult> ExecuteAsync(Guid businessId, string phoneNumber)
        {
            var normalizedPhone = PhoneSanitizer.Sanitize(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                return new ToolResult { Message = "No encontré reparaciones registradas con este número." };
            }

            var orders = await _context.RepairOrders
                .Include(r => r.Contact)
                .Where(r =>
                    r.BusinessId == businessId &&
                    r.Contact.Phone == normalizedPhone)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (!orders.Any())
            {
                return new ToolResult
                {
                    Message = "No encontré reparaciones registradas con este número."
                };
            }

            var activeOrders = orders
                .Where(o => o.Status != 3)
                .ToList();

            if (activeOrders.Any())
            {
                var message = "Encontré estas reparaciones activas:\n\n";

                foreach (var o in activeOrders.Take(3))
                {
                    var status = GetStatus(o.Status);

                    message += $"• {o.DeviceDescription} — {status}\n";
                }

                return new ToolResult
                {
                    Message = message
                };
            }            

            var lastOrder = orders.First();

            return new ToolResult
            {
                Message = $"Tu última reparación ({lastOrder.DeviceDescription}) ya fue entregada."
            };
        }
        private static string GetStatus(int status)
        {
            return status switch
            {
                0 => "Pendiente",
                1 => "En proceso",
                2 => "Finalizada",
                3 => "Entregada",
                _ => "Desconocido"
            };
        }
    }
}
