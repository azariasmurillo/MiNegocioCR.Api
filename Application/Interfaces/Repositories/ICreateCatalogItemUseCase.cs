using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateCatalogItemUseCase
    {
        Task<Guid> ExecuteAsync(
            Guid businessId,
            string name,
            decimal basePrice,
            bool trackStock,
            CatalogItemType type);
    }
}
