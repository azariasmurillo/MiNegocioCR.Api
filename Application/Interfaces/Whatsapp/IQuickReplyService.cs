namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IQuickReplyService
    {
        Task SendTemplateAsync(
            Guid businessId,
            string phone,
            Guid templateId);
    }
}
