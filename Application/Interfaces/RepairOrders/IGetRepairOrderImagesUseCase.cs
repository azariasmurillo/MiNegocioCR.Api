using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface IGetRepairOrderImagesUseCase
{
    Task<IReadOnlyList<RepairOrderImageDto>> ExecuteAsync(Guid businessId, Guid repairOrderId, CancellationToken cancellationToken = default);
}
