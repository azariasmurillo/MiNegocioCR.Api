using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class CreateOptionValueUseCase : ICreateOptionValueUseCase
    {
        private readonly ICatalogOptionValueRepository _valueRepository;
        private readonly ICatalogOptionRepository _optionRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IBusinessDimensionValueRepository _dimensionLibraryRepository;

        public CreateOptionValueUseCase(
            ICatalogOptionValueRepository valueRepository,
            ICatalogOptionRepository optionRepository,
            ICatalogRepository catalogRepository,
            IBusinessDimensionValueRepository dimensionLibraryRepository)
        {
            _valueRepository = valueRepository;
            _optionRepository = optionRepository;
            _catalogRepository = catalogRepository;
            _dimensionLibraryRepository = dimensionLibraryRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateCatalogOptionValueRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.OptionId == Guid.Empty)
                throw new ArgumentException("OptionId is required.", nameof(request));

            var option = await _optionRepository.GetByIdAsync(request.OptionId);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            var normalizedValue = CatalogDimensionRules.NormalizeDimensionValue(request.Value, option.Name);

            if (await _valueRepository.ExistsActiveValueAsync(option.Id, normalizedValue))
            {
                throw new ArgumentException(
                    $"Ya existe el valor «{normalizedValue}» en {option.Name}.",
                    nameof(request));
            }

            var entity = new CatalogOptionValue
            {
                Id = Guid.NewGuid(),
                CatalogOptionId = request.OptionId,
                Value = normalizedValue,
                IsActive = true
            };

            await _valueRepository.AddAsync(entity);

            var item = await _catalogRepository.GetItemByIdAsync(option.CatalogItemId);
            if (item != null)
            {
                await _dimensionLibraryRepository.UpsertAsync(new BusinessDimensionValue
                {
                    Id = Guid.NewGuid(),
                    BusinessId = item.BusinessId,
                    DimensionName = option.Name,
                    Value = normalizedValue,
                    ValueKey = CatalogDimensionRules.BuildValueKey(option.Name, normalizedValue),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            return entity.Id;
        }
    }
}
