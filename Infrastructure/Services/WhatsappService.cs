using MiNegocioCR.Api.API.Controllers;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class WhatsappService : IWhatsappService
    {
        private readonly HttpClient _httpClient;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<WhatsappWebhookController> _logger;

        public WhatsappService(HttpClient httpClient, IEncryptionService _encryptionService, ILogger<WhatsappWebhookController> logger)
        {
            _httpClient = httpClient;
            this._encryptionService = _encryptionService;
            _logger = logger;
        }        
        
        public async Task SendAsync(GetBusinessByIdResultDto business, string phone, string message, string? attachmentUrl = null, string? attachmentType = null)
        {
            if (string.IsNullOrWhiteSpace(business.WhatsappPhoneNumberId) ||
                string.IsNullOrWhiteSpace(business.WhatsappAccessToken))
                throw new WhatsappNotConfiguredException();

            var url = $"https://graph.facebook.com/v19.0/{business.WhatsappPhoneNumberId}/messages";

            object payload;

            if (!string.IsNullOrEmpty(attachmentUrl) && !string.IsNullOrEmpty(attachmentType))
            {
                payload = new
                {
                    messaging_product = "whatsapp",
                    to = phone,
                    type = attachmentType,
                    image = attachmentType == "image" ? new { link = attachmentUrl } : null,
                    document = attachmentType == "document" ? new { link = attachmentUrl } : null,
                    audio = attachmentType == "audio" ? new { link = attachmentUrl } : null,
                    video = attachmentType == "video" ? new { link = attachmentUrl } : null
                };
            }
            else
            {
                payload = new
                {
                    messaging_product = "whatsapp",
                    to = phone,
                    type = "text",
                    text = new { body = message }
                };
            }


            var decryptedToken = _encryptionService.Decrypt(business.WhatsappAccessToken);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", decryptedToken);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Token usado: {token}", decryptedToken);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Whatsapp API error: {error}");
            }
        }

        public async Task<bool> ValidateAsync(string phoneNumberId, string accessToken)
        {
            var url = $"https://graph.facebook.com/v19.0/{phoneNumberId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
