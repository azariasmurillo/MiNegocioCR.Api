using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class GetBusinessDimensionValuesUseCase : IGetBusinessDimensionValuesUseCase
    {
        private readonly IBusinessDimensionValueRepository _repository;

        public GetBusinessDimensionValuesUseCase(IBusinessDimensionValueRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<BusinessDimensionValueDto>> ExecuteAsync(
            Guid businessId,
            string dimensionName,
            bool includeInactive = false)
        {
            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("BusinessId is required.", nameof(businessId));
            }

            if (string.IsNullOrWhiteSpace(dimensionName))
            {
                throw new ArgumentException("Dimension name is required.", nameof(dimensionName));
            }

            var canonical = CatalogDimensionRules.IsStandardDimension(dimensionName)
                ? CatalogDimensionRules.ValidateAndNormalizeDimensionName(dimensionName, isCustomDimension: false)
                : CatalogDimensionRules.ValidateAndNormalizeDimensionName(dimensionName, isCustomDimension: true);

            var rows = await _repository.GetByBusinessAndDimensionAsync(businessId, canonical, includeInactive);
            return rows
                .Select(x => new BusinessDimensionValueDto
                {
                    Id = x.Id,
                    DimensionName = x.DimensionName,
                    Value = x.Value,
                })
                .ToList();
        }
    }

    public class GetCatalogDimensionCatalogUseCase : IGetCatalogDimensionCatalogUseCase
    {
        public CatalogDimensionCatalogDto Execute()
        {
            return new CatalogDimensionCatalogDto
            {
                StandardDimensions = CatalogDimensionRules.StandardDimensionNames,
                MaxDimensionsPerProduct = CatalogDimensionRules.MaxDimensionsPerProduct,
            };
        }
    }
}
