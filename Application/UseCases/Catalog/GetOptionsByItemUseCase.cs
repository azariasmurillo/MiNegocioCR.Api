using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class GetOptionsByItemUseCase : IGetOptionsByItemUseCase
    {
        private readonly ICatalogOptionRepository _optionRepository;
        private readonly ICatalogRepository _catalogRepository;

        public GetOptionsByItemUseCase(
            ICatalogOptionRepository optionRepository,
            ICatalogRepository catalogRepository)
        {
            _optionRepository = optionRepository;
            _catalogRepository = catalogRepository;
        }

        public async Task<IReadOnlyList<CatalogOptionDto>> ExecuteAsync(Guid catalogItemId, bool includeInactive = false)
        {
            if (catalogItemId == Guid.Empty)
                throw new ArgumentException("CatalogItemId is required.", nameof(catalogItemId));

            var item = await _catalogRepository.GetItemByIdAsync(catalogItemId);
            if (item == null)
                throw new NotFoundException("CatalogItem", "Catalog item not found.");

            var entities = await _optionRepository.GetByCatalogItemIdAsync(catalogItemId, includeInactive);

            return entities
                .Select(x => new CatalogOptionDto
                {
                    Id = x.Id,
                    CatalogItemId = x.CatalogItemId,
                    Name = x.Name,
                    IsActive = x.IsActive
                })
                .ToList();
        }
    }
}
