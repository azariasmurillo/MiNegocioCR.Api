namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappApplicationService
    {
        Task SendAsync(Guid businessId, string phone, string message, string? attachmentUrl = null
            ,string? attachmentType = null);
        Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken
            , CancellationToken cancellationToken = default);
    }
}
