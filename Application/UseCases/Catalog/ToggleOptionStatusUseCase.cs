using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class ToggleOptionStatusUseCase : IToggleOptionStatusUseCase
    {
        private readonly ICatalogOptionRepository _optionRepository;

        public ToggleOptionStatusUseCase(ICatalogOptionRepository optionRepository)
        {
            _optionRepository = optionRepository;
        }

        public async Task ExecuteAsync(Guid id, ToggleOptionStatusRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            var option = await _optionRepository.GetByIdAsync(id);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            option.IsActive = request.IsActive;
            await _optionRepository.UpdateAsync(option);
        }
    }
}
