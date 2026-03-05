using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using System.Text.Json;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class WhatsappMessageService : IWhatsappMessageService
    {
        private readonly IWhatsappMessageRepository _messageRepository;
        private readonly IBusinessRepository _businessRepository;

        public WhatsappMessageService(IWhatsappMessageRepository repository, 
            IBusinessRepository businessRepository)
        {
            _messageRepository = repository;
            _businessRepository = businessRepository;
        }

        public async Task ProcessWebhookAsync(JsonElement payload)
        {
            var entry = payload.GetProperty("entry")[0];
            var changes = entry.GetProperty("changes")[0];
            var value = changes.GetProperty("value");

            if (value.TryGetProperty("messages", out var messages))
            {
                await ProcessIncomingMessage(messages, value);
            }

            if (value.TryGetProperty("statuses", out var statuses))
            {
                await ProcessStatusUpdate(statuses);
            }
        }

        private async Task ProcessIncomingMessage(JsonElement messages, JsonElement value)
        {
            var message = messages[0];

            var messageId = message.GetProperty("id").GetString();
            var from = message.GetProperty("from").GetString();
            var timestamp = message.GetProperty("timestamp").GetString();

            var body = message.GetProperty("text")
                              .GetProperty("body")
                              .GetString();

            var phoneNumberId = value.GetProperty("metadata")
                                     .GetProperty("phone_number_id")
                                     .GetString();

            var business = await _businessRepository.GetByWhatsappPhoneNumberIdAsync(phoneNumberId!);

            if (business == null)
            {
                return; 
            }

            var entity = new WhatsAppMessage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                MessageId = messageId!,
                PhoneNumber = from!,
                From = from!,
                To = phoneNumberId!,
                Body = body!,
                Timestamp = DateTime.UtcNow,
                Direction = MessageDirection.Inbound,
                Status = MessageStatus.Received
            };

            await _messageRepository.SaveAsync(entity);
        }

        private async Task ProcessStatusUpdate(JsonElement statuses)
        {
            var status = statuses[0];

            var messageId = status.GetProperty("id").GetString();
            var statusValue = status.GetProperty("status").GetString();

            MessageStatus newStatus = statusValue switch
            {
                "delivered" => MessageStatus.Delivered,
                "read" => MessageStatus.Read,
                "failed" => MessageStatus.Failed,
                _ => MessageStatus.Sent
            };

            await _messageRepository.UpdateStatusAsync(messageId!, newStatus);
        }
    }
}
