using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class CreateOptionUseCase : ICreateOptionUseCase
    {
        private readonly ICatalogOptionRepository _optionRepository;
        private readonly ICatalogRepository _catalogRepository;

        public CreateOptionUseCase(
            ICatalogOptionRepository optionRepository,
            ICatalogRepository catalogRepository)
        {
            _optionRepository = optionRepository;
            _catalogRepository = catalogRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateCatalogOptionRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.CatalogItemId == Guid.Empty)
                throw new ArgumentException("CatalogItemId is required.", nameof(request));

            var item = await _catalogRepository.GetItemByIdAsync(request.CatalogItemId);
            if (item == null)
                throw new NotFoundException("CatalogItem", "Catalog item not found.");

            var dimensionName = CatalogDimensionRules.ValidateAndNormalizeDimensionName(
                request.Name,
                request.IsCustomDimension);

            var activeCount = await _optionRepository.CountActiveByCatalogItemIdAsync(request.CatalogItemId);
            if (activeCount >= CatalogDimensionRules.MaxDimensionsPerProduct)
            {
                throw new ArgumentException(
                    $"Un producto puede tener como máximo {CatalogDimensionRules.MaxDimensionsPerProduct} dimensiones.",
                    nameof(request));
            }

            if (await _optionRepository.ExistsActiveNameOnItemAsync(request.CatalogItemId, dimensionName))
            {
                throw new ArgumentException(
                    $"Ya existe la dimensión «{dimensionName}» en este producto.",
                    nameof(request));
            }

            var option = new CatalogOption
            {
                Id = Guid.NewGuid(),
                CatalogItemId = request.CatalogItemId,
                Name = dimensionName,
                IsActive = true
            };

            await _optionRepository.AddAsync(option);

            return option.Id;
        }
    }
}
