using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class WhatsappService : IWhatsappService
{
    private readonly HttpClient _httpClient;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<WhatsappService> _logger;

    public WhatsappService(
        HttpClient httpClient,
        IEncryptionService encryptionService,
        ILogger<WhatsappService> logger)
    {
        _httpClient = httpClient;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task SendAsync(GetBusinessByIdResultDto business, string phone, string message,
        string? attachmentUrl = null, string? attachmentType = null)
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
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", decryptedToken);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        _logger.LogDebug(
            "WhatsApp Graph send prepared. Token: {TokenHint}, PhoneNumberId: {PhoneNumberId}",
            TokenLogMask.MaskLastFour(decryptedToken),
            business.WhatsappPhoneNumberId);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "WhatsApp Graph API send failed. Status: {Status}, PhoneNumberId: {PhoneNumberId}",
                response.StatusCode, business.WhatsappPhoneNumberId);
            throw CreateApiExceptionFromBody(errorBody, response.StatusCode);
        }
    }

    public async Task<bool> ValidateAsync(string phoneNumberId, string accessToken)
    {
        var url = $"https://graph.facebook.com/v19.0/{phoneNumberId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        return response.IsSuccessStatusCode;
    }

    private static WhatsappApiException CreateApiExceptionFromBody(string errorBody, HttpStatusCode status)
    {
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var err))
            {
                var code = err.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.Number
                    ? c.GetInt32()
                    : (int?)null;
                var sub = err.TryGetProperty("error_subcode", out var s) && s.ValueKind == JsonValueKind.Number
                    ? s.GetInt32()
                    : (int?)null;
                var type = err.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String
                    ? t.GetString()
                    : null;
                var msg = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                var message = string.IsNullOrEmpty(msg)
                    ? $"Graph API error (HTTP {(int)status})"
                    : msg!;
                return new WhatsappApiException(message, code, sub, type, errorBody);
            }
        }
        catch (JsonException)
        {
            // fall through
        }

        return new WhatsappApiException(
            $"WhatsApp Graph API error (HTTP {(int)status}): {errorBody}",
            code: null,
            subcode: null,
            rawResponse: errorBody);
    }
}
