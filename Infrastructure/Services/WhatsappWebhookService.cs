using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                    if (!change.TryGetProperty("value", out var value))
                        continue;
                    
                    if (value.TryGetProperty("messages", out var messages))
                    {
                        foreach (var msg in messages.EnumerateArray())
                        {
                            var metaId = msg.GetProperty("id").GetString();
                            var from = msg.GetProperty("from").GetString();

                            string? body = null;

                            if (msg.TryGetProperty("text", out var textObj))
                            {
                                body = textObj.GetProperty("body").GetString();
                            }
                                                       
                            var exists = await _context.WhatsappMessages
                                .AnyAsync(x => x.MetaMessageId == metaId, cancellationToken);

                            if (exists)
                                continue;

                            var entity = new WhatsappMessage
                            {
                                MetaMessageId = metaId,
                                FromNumber = from,
                                Body = body,
                                Status = "received",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.WhatsappMessages.Add(entity);
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    
                    if (value.TryGetProperty("statuses", out var statuses))
                    {
                        foreach (var statusObj in statuses.EnumerateArray())
                        {
                            var messageId = statusObj.GetProperty("id").GetString();
                            var messageStatus = statusObj.GetProperty("status").GetString();

                            var message = await _context.WhatsappMessages
                                .FirstOrDefaultAsync(x => x.MetaMessageId == messageId, cancellationToken);

                            if (message == null)
                                continue;

                            message.Status = messageStatus;
                            message.UpdatedAt = DateTime.UtcNow;
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }
    }
}