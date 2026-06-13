using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class UpdateOptionUseCase : IUpdateOptionUseCase
    {
        private readonly ICatalogOptionRepository _optionRepository;

        public UpdateOptionUseCase(ICatalogOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public async Task ExecuteAsync(Guid id, UpdateOptionRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            var option = await _optionRepository.GetByIdAsync(id);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            var dimensionName = CatalogDimensionRules.ValidateAndNormalizeDimensionName(
                request.Name,
                request.IsCustomDimension);

            if (await _optionRepository.ExistsActiveNameOnItemAsync(option.CatalogItemId, dimensionName, id))
            {
                throw new ArgumentException(
                    $"Ya existe la dimensión «{dimensionName}» en este producto.",
                    nameof(request));
            }

            option.Name = dimensionName;
            await _optionRepository.UpdateAsync(option);
        }
    }
}
