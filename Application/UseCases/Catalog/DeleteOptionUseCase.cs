using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class DeleteOptionUseCase : IDeleteOptionUseCase
    {
        private readonly ICatalogOptionRepository _optionRepository;

        public DeleteOptionUseCase(ICatalogOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public async Task ExecuteAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            var option = await _optionRepository.GetByIdAsync(id);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            var hasValues = await _optionRepository.ExistsWithValuesAsync(id);
            if (hasValues)
                throw new InvalidOperationException("Option has values and cannot be deleted");

            await _optionRepository.DeleteAsync(option);
        }
    }
}
