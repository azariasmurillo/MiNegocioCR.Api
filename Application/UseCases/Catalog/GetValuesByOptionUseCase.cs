using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class GetValuesByOptionUseCase : IGetValuesByOptionUseCase
    {
        private readonly ICatalogOptionValueRepository _valueRepository;
        private readonly ICatalogOptionRepository _optionRepository;

        public GetValuesByOptionUseCase(
            ICatalogOptionValueRepository valueRepository,
            ICatalogOptionRepository optionRepository)
        {
            _valueRepository = valueRepository;
            _optionRepository = optionRepository;
        }

        public async Task<IReadOnlyList<CatalogOptionValueDto>> ExecuteAsync(Guid optionId)
        {
            if (optionId == Guid.Empty)
                throw new ArgumentException("OptionId is required.", nameof(optionId));

            var option = await _optionRepository.GetByIdAsync(optionId);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            var entities = await _valueRepository.GetByCatalogOptionIdAsync(optionId);

            return entities
                .Select(x => new CatalogOptionValueDto
                {
                    Id = x.Id,
                    OptionId = x.CatalogOptionId,
                    Value = x.Value
                })
                .ToList();
        }
    }
}
