namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface IDeleteRepairOrderImageUseCase
{
    Task ExecuteAsync(Guid businessId, Guid imageId, CancellationToken cancellationToken = default);
}
