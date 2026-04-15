using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class DeleteCatalogItemUseCase : IDeleteCatalogItemUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public DeleteCatalogItemUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task ExecuteAsync(Guid id, Guid businessId)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            var item = await _catalogRepository.GetItemAsync(id, businessId);
            if (item == null)
                throw new NotFoundException("CatalogItem", "Catalog item not found.");

            var hasVariants = await _catalogRepository.ExistsWithVariantsAsync(id);
            if (hasVariants)
                throw new InvalidOperationException("CatalogItem has variants and cannot be deleted");

            item.IsActive = false;
            await _catalogRepository.UpdateAsync(item);
        }
    }
}
