using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class UpdateOptionValueUseCase : IUpdateOptionValueUseCase
    {
        private readonly ICatalogOptionValueRepository _valueRepository;
        private readonly ICatalogOptionRepository _optionRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IBusinessDimensionValueRepository _dimensionLibraryRepository;

        public UpdateOptionValueUseCase(
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

        public async Task ExecuteAsync(Guid id, UpdateOptionValueRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            var optionValue = await _valueRepository.GetByIdAsync(id);
            if (optionValue == null)
                throw new NotFoundException("CatalogOptionValue", "Option value not found.");

            var option = await _optionRepository.GetByIdAsync(optionValue.CatalogOptionId);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            var normalizedValue = CatalogDimensionRules.NormalizeDimensionValue(request.Value, option.Name);

            if (await _valueRepository.ExistsActiveValueAsync(option.Id, normalizedValue, id))
            {
                throw new ArgumentException(
                    $"Ya existe el valor «{normalizedValue}» en {option.Name}.",
                    nameof(request));
            }

            optionValue.Value = normalizedValue;
            await _valueRepository.UpdateAsync(optionValue);

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
        }
    }
}
