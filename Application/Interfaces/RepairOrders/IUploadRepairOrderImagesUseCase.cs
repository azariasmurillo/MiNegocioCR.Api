using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface IUploadRepairOrderImagesUseCase
{
    Task<IReadOnlyList<RepairOrderImageDto>> ExecuteAsync(
        Guid businessId,
        Guid repairOrderId,
        IReadOnlyList<RepairOrderImageUploadInput> files,
        CancellationToken cancellationToken = default);
}
