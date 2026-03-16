namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IGetUnreadTotalUseCase
    {
        Task<int> Execute(Guid businessId);
    }
}
