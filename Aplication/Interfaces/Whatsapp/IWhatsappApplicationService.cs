namespace MiNegocioCR.Api.Aplication.Interfaces.Whatsapp
{
    public interface IWhatsappApplicationService
    {
        Task SendAsync(Guid businessId, string phone, string message);
        Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken, CancellationToken cancellationToken = default);
    }
}
