namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappWebhookLogRepository
    {
        Task SaveAsync(string payload);
    }
}
