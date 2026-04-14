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

        public CreateOptionValueUseCase(
            ICatalogOptionValueRepository valueRepository,
            ICatalogOptionRepository optionRepository)
        {
            _valueRepository = valueRepository;
            _optionRepository = optionRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateCatalogOptionValueRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.OptionId == Guid.Empty)
                throw new ArgumentException("OptionId is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Value))
                throw new ArgumentException("Value is required.", nameof(request));

            var option = await _optionRepository.GetByIdAsync(request.OptionId);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            var entity = new CatalogOptionValue
            {
                Id = Guid.NewGuid(),
                CatalogOptionId = request.OptionId,
                Value = request.Value.Trim()
            };

            await _valueRepository.AddAsync(entity);

            return entity.Id;
        }
    }
}
