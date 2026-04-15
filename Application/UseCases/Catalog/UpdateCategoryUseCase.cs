using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class UpdateCategoryUseCase : IUpdateCategoryUseCase
    {
        private readonly ICatalogCategoryRepository _categoryRepository;

        public UpdateCategoryUseCase(ICatalogCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task ExecuteAsync(Guid categoryId, UpdateCategoryRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId is required.", nameof(categoryId));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.", nameof(request));

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException("CatalogCategory", "Category not found.");

            if (category.BusinessId != request.BusinessId)
                throw new ArgumentException("Category does not belong to the same business.", nameof(request));

            category.Name = request.Name.Trim();

            await _categoryRepository.UpdateAsync(category);
        }
    }
}
