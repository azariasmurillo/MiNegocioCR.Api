
using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.Whatsapp
{
    public interface IWhatsappService
    {
        Task SendAsync(GetBusinessByIdResultDto business, string phone, string message);
        Task<bool> ValidateAsync(string phoneNumberId, string accessToken);
    }
}
