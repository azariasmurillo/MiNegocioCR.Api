using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class DeleteOptionValueUseCase : IDeleteOptionValueUseCase
    {
        private readonly ICatalogOptionValueRepository _valueRepository;

        public DeleteOptionValueUseCase(ICatalogOptionValueRepository valueRepository)
        {
            _valueRepository = valueRepository;
        }

        public async Task ExecuteAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            var optionValue = await _valueRepository.GetByIdAsync(id);
            if (optionValue == null)
                throw new NotFoundException("CatalogOptionValue", "Option value not found.");

            var inVariants = await _valueRepository.ExistsInVariantsAsync(id);
            if (inVariants)
                throw new InvalidOperationException("OptionValue is used in variants");

            await _valueRepository.DeleteAsync(optionValue);
        }
    }
}
