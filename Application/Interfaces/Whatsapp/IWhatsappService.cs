
using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappService
    {
        Task SendAsync(GetBusinessByIdResultDto business, string phone, string message, string? attachmentUrl = null,
    string? attachmentType = null);
        Task<bool> ValidateAsync(string phoneNumberId, string accessToken);
    }
}
