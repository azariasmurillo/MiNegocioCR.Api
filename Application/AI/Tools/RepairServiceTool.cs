using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.AI.Search;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.Tools
{
    public class RepairServiceTool : IAITool
    {
        private readonly AppDbContext _context;

        public string Name => "repair_service_search";

        public RepairServiceTool(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ToolResult> ExecuteAsync(Guid businessId, string query)
        {
            query = query.ToLower().Trim();

            var normalizer = new SearchNormalizer();
            var terms = normalizer.Normalize(query);

            var services = await _context.CatalogItems
                .Where(i =>
                i.BusinessId == businessId &&
                i.Type == CatalogItemType.Service)
                .ToListAsync();

            var matches = services
                .Where(s =>
                    terms.Any(term =>
                        s.Name.ToLower().Contains(term)))
                .ToList();

            if (matches.Count == 0)
            {
                return new ToolResult
                {
                    Message = "Ofrecemos servicios de reparación. ¿Qué dispositivo deseas reparar?"
                };
            }

            var topServices = matches
                .OrderByDescending(s => terms.Any(t => s.Name.ToLower().Contains(t)))
                .Take(5)
                .ToList();

            var message = "Podemos ayudarte con estos servicios:\n\n";

            foreach (var s in topServices)
            {
                message += $"• {s.Name} (desde ₡{s.BasePrice:N0})\n";
            }

            message += "\n\nPuedes visitarnos en el taller si necesitas alguno de estos servicios.";

            return new ToolResult
            {
                Message = message
            };
        }
    }
}
