namespace MiNegocioCR.Api.Application.Interfaces.InternetOrders;

public interface ISendInternetOrderEmailUseCase
{
    Task Execute(Guid businessId, Guid orderId, string htmlContent, string? destinationEmail = null);
}
