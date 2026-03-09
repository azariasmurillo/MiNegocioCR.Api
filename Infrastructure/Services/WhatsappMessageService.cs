using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
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
        private readonly IAIService _aiService;
        private readonly IWhatsappApplicationService _whatsappAppService;
        private readonly ILogger<WhatsappMessageService> _logger;

        public WhatsappMessageService(
            IWhatsappMessageRepository repository,
            IBusinessRepository businessRepository,
            IAIService aiService,
            IWhatsappApplicationService whatsappAppService,
            ILogger<WhatsappMessageService> logger)
        {
            _messageRepository = repository;
            _businessRepository = businessRepository;
            _aiService = aiService;
            _whatsappAppService = whatsappAppService;
            _logger = logger;
        }

        public async Task ProcessWebhookAsync(JsonElement payload)
        {
            if (!payload.TryGetProperty("entry", out var entryArr) || entryArr.GetArrayLength() == 0)
            {
                _logger.LogDebug("[WhatsApp] Webhook sin entry o entry vacío, se ignora.");
                return;
            }
            var entry = entryArr[0];

            if (!entry.TryGetProperty("changes", out var changesArr) || changesArr.GetArrayLength() == 0)
                return;
            var changes = changesArr[0];

            if (!changes.TryGetProperty("value", out var value))
                return;

            if (value.TryGetProperty("messages", out var messages) && messages.GetArrayLength() > 0)
            {
                _logger.LogInformation("[WhatsApp] Webhook con mensaje(s), procesando.");
                await ProcessIncomingMessage(messages, value);
            }

            if (value.TryGetProperty("statuses", out var statuses) && statuses.GetArrayLength() > 0)
            {
                _logger.LogDebug("[WhatsApp] Webhook con status(es), actualizando.");
                await ProcessStatusUpdate(statuses);
            }
        }

        private async Task ProcessIncomingMessage(JsonElement messages, JsonElement value)
        {
            if (messages.GetArrayLength() == 0)
                return;
            var message = messages[0];

            var messageId = message.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var from = message.TryGetProperty("from", out var fromProp) ? fromProp.GetString() : null;
            if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(from))
                return;

            var body = "";
            if (message.TryGetProperty("text", out var textObj) && textObj.TryGetProperty("body", out var bodyProp))
                body = bodyProp.GetString() ?? "";

            if (!value.TryGetProperty("metadata", out var metadata) || !metadata.TryGetProperty("phone_number_id", out var phoneProp))
                return;
            var phoneNumberId = phoneProp.GetString();
            if (string.IsNullOrEmpty(phoneNumberId))
                return;

            var business = await _businessRepository.GetByWhatsappPhoneNumberIdAsync(phoneNumberId);

            if (business == null)
            {
                _logger.LogWarning("[WhatsApp] Mensaje entrante desde {From}, texto: \"{Body}\". Negocio no encontrado para phone_number_id: {PhoneNumberId}", from, body, phoneNumberId);
                return;
            }

            _logger.LogInformation("[WhatsApp] Mensaje entrante. BusinessId: {BusinessId}, From: {From}, Texto: \"{Body}\"", business.Id, from, body);

            var entity = new WhatsAppMessage
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                MessageId = messageId,
                PhoneNumber = from,
                From = from,
                To = phoneNumberId,
                Body = body,
                Timestamp = DateTime.UtcNow,
                Direction = MessageDirection.Inbound,
                Status = MessageStatus.Received
            };

            await _messageRepository.SaveAsync(entity);

            // Conectar IA: obtener respuesta y enviarla por WhatsApp
            try
            {
                var aiRequest = new AIRequest
                {
                    BusinessId = business.Id,
                    UserMessage = body,
                    PhoneNumber = from,
                    Channel = "whatsapp"
                };

                var response = await _aiService.AskAsync(aiRequest);
                _logger.LogInformation("AI RESPONSE: {Response}", response);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    await _whatsappAppService.SendAsync(business.Id, from, response);
                    _logger.LogInformation("[WhatsApp] Respuesta IA enviada a {From}, longitud: {Len}", from, response.Length);
                }
                else
                {
                    _logger.LogDebug("[WhatsApp] IA devolvió respuesta vacía (ej. chat deshabilitado), no se envía mensaje.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WhatsApp] Error al procesar IA o enviar respuesta. BusinessId: {BusinessId}, From: {From}", business.Id, from);
            }
        }

        private async Task ProcessStatusUpdate(JsonElement statuses)
        {
            if (statuses.GetArrayLength() == 0)
                return;
            var status = statuses[0];

            var messageId = status.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var statusValue = status.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
            if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(statusValue))
                return;

            MessageStatus newStatus = statusValue switch
            {
                "delivered" => MessageStatus.Delivered,
                "read" => MessageStatus.Read,
                "failed" => MessageStatus.Failed,
                _ => MessageStatus.Sent
            };

            await _messageRepository.UpdateStatusAsync(messageId, newStatus);
        }
    }
}
