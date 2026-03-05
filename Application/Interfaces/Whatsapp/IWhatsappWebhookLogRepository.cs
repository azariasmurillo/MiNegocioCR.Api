namespace MiNegocioCR.Api.Aplication.Interfaces.Whatsapp
{
    public interface IWhatsappWebhookLogRepository
    {
        Task SaveAsync(string payload);
    }
}
