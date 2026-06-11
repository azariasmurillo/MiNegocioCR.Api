using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class GetCatalogItemsByBusinessUseCase : IGetCatalogItemsByBusinessUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetCatalogItemsByBusinessUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IReadOnlyList<CatalogItemDto>> ExecuteAsync(Guid businessId, bool includeInactive = false)
        {
            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            var items = await _catalogRepository.GetItemsAsync(businessId, includeInactive);

            return items
                .Select(x => new CatalogItemDto
                {
                    Id = x.Id,
                    BusinessId = x.BusinessId,
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    Description = x.Description,
                    BasePrice = x.BasePrice,
                    TrackStock = x.TrackStock,
                    Type = x.Type,
                    IsActive = x.IsActive
                })
                .ToList();
        }
    }
}
