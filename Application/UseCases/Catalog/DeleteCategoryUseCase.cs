using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Catalog
{
    public class DeleteCategoryUseCase : IDeleteCategoryUseCase
    {
        private readonly ICatalogCategoryRepository _categoryRepository;

        public DeleteCategoryUseCase(ICatalogCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task ExecuteAsync(Guid categoryId, Guid businessId)
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId is required.", nameof(categoryId));

            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException("CatalogCategory", "Category not found.");

            if (category.BusinessId != businessId)
                throw new ArgumentException("Category does not belong to the same business.", nameof(businessId));

            var inUse = await _categoryRepository.ExistsWithProductsAsync(categoryId);
            if (inUse)
                throw new InvalidOperationException("Category is in use and cannot be deleted");

            category.IsActive = false;
            await _categoryRepository.UpdateAsync(category);
        }
    }
}
