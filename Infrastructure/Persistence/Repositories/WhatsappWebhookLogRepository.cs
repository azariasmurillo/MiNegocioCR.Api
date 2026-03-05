using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class WhatsappWebhookLogRepository : IWhatsappWebhookLogRepository
    {
        private readonly AppDbContext _context;

        public WhatsappWebhookLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(string payload)
        {
            var log = new WhatsappWebhookLog
            {
                Id = Guid.NewGuid(),
                Payload = payload
            };

            _context.WhatsappWebhookLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}
