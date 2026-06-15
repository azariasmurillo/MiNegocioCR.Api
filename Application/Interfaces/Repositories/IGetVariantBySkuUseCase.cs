using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories;

public interface IGetVariantBySkuUseCase
{
    Task<VariantBySkuLookupDto> ExecuteAsync(Guid businessId, string sku, CancellationToken cancellationToken = default);
}
