using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class ToggleCatalogItemStatusUseCase : IToggleCatalogItemStatusUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public ToggleCatalogItemStatusUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task ExecuteAsync(Guid id, ToggleCatalogItemStatusRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            var item = await _catalogRepository.GetItemAsync(id, request.BusinessId);
            if (item == null)
                throw new NotFoundException("CatalogItem", "Catalog item not found.");

            item.IsActive = request.IsActive;

            await _catalogRepository.UpdateAsync(item);
        }
    }
}
