namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface ISendRepairOrderEmailUseCase
{
    Task Execute(Guid id, string htmlContent, string? destinationEmail = null);
}
