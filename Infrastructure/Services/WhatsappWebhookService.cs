using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Persistence;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class WhatsappWebhookService : IWhatsappWebhookService
    {
        private readonly AppDbContext _context;

        public WhatsappWebhookService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ProcessAsync(JsonElement payload, CancellationToken cancellationToken)
        {
            if (!payload.TryGetProperty("entry", out var entries))
                return;

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("changes", out var changes))
                    continue;

                foreach (var change in changes.EnumerateArray())
                {
                    var value = change.GetProperty("value");

                    if (value.TryGetProperty("statuses", out var statuses))
                    {
                        foreach (var status in statuses.EnumerateArray())
                        {
                            var messageId = status.GetProperty("id").GetString();
                            var messageStatus = status.GetProperty("status").GetString();

                            await UpdateMessageStatus(messageId, messageStatus, cancellationToken);
                        }
                    }
                }
            }
        }

        private async Task UpdateMessageStatus(string messageId, string status, CancellationToken cancellationToken)
        {
            var message = await _context.WhatsappMessages
                .FirstOrDefaultAsync(x => x.MetaMessageId == messageId);

            if (message == null)
                return;

            message.Status = status;
            message.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
