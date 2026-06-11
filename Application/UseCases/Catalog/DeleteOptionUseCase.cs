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

            var inVariants = await _optionRepository.ExistsInVariantsAsync(id);
            if (inVariants)
                throw new InvalidOperationException("Hay presentaciones que usan esta dimensión; no se puede eliminar.");

            await _optionRepository.DeleteAsync(option);
        }
    }
}
