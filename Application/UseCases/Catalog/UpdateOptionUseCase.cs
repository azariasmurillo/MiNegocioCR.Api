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

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.", nameof(request));

            var option = await _optionRepository.GetByIdAsync(id);
            if (option == null)
                throw new NotFoundException("CatalogOption", "Catalog option not found.");

            option.Name = request.Name.Trim();
            await _optionRepository.UpdateAsync(option);
        }
    }
}
