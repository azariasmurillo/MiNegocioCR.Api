using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class GetCategoriesByBusinessUseCase : IGetCategoriesByBusinessUseCase
    {
        private readonly ICatalogCategoryRepository _categoryRepository;

        public GetCategoriesByBusinessUseCase(ICatalogCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IReadOnlyList<CatalogCategoryDto>> ExecuteAsync(Guid businessId, bool includeInactive = false)
        {
            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            var entities = await _categoryRepository.GetByBusinessIdAsync(businessId, includeInactive);

            return entities
                .Select(x => new CatalogCategoryDto
                {
                    Id = x.Id,
                    BusinessId = x.BusinessId,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .ToList();
        }
    }
}
