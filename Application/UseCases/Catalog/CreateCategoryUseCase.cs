using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class CreateCategoryUseCase : ICreateCategoryUseCase
    {
        private readonly ICatalogCategoryRepository _categoryRepository;

        public CreateCategoryUseCase(ICatalogCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateCategoryRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.", nameof(request));

            var category = new CatalogCategory
            {
                Id = Guid.NewGuid(),
                BusinessId = request.BusinessId,
                Name = request.Name.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);

            return category.Id;
        }
    }
}
