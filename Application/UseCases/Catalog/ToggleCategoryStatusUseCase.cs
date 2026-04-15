using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class ToggleCategoryStatusUseCase : IToggleCategoryStatusUseCase
    {
        private readonly ICatalogCategoryRepository _categoryRepository;

        public ToggleCategoryStatusUseCase(ICatalogCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task ExecuteAsync(Guid categoryId, ToggleCategoryStatusRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId is required.", nameof(categoryId));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException("CatalogCategory", "Category not found.");

            if (category.BusinessId != request.BusinessId)
                throw new ArgumentException("Category does not belong to the same business.", nameof(request));

            category.IsActive = request.IsActive;

            await _categoryRepository.UpdateAsync(category);
        }
    }
}
