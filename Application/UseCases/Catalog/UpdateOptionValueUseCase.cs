using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class UpdateOptionValueUseCase : IUpdateOptionValueUseCase
    {
        private readonly ICatalogOptionValueRepository _valueRepository;

        public UpdateOptionValueUseCase(ICatalogOptionValueRepository valueRepository)
        {
            _valueRepository = valueRepository;
        }

        public async Task ExecuteAsync(Guid id, UpdateOptionValueRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            if (string.IsNullOrWhiteSpace(request.Value))
                throw new ArgumentException("Value is required.", nameof(request));

            var optionValue = await _valueRepository.GetByIdAsync(id);
            if (optionValue == null)
                throw new NotFoundException("CatalogOptionValue", "Option value not found.");

            optionValue.Value = request.Value.Trim();
            await _valueRepository.UpdateAsync(optionValue);
        }
    }
}
