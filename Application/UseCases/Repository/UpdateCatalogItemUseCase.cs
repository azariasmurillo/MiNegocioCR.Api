using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class UpdateCatalogItemUseCase : IUpdateCatalogItemUseCase
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICatalogCategoryRepository _categoryRepository;

        public UpdateCatalogItemUseCase(
            ICatalogRepository catalogRepository,
            ICatalogCategoryRepository categoryRepository)
        {
            _catalogRepository = catalogRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task ExecuteAsync(Guid id, UpdateCatalogItemRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(id));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.", nameof(request));

            var item = await _catalogRepository.GetItemAsync(id, request.BusinessId);
            if (item == null)
                throw new NotFoundException("CatalogItem", "Catalog item not found.");

            if (request.CategoryId.HasValue)
            {
                if (request.CategoryId.Value == Guid.Empty)
                    throw new ArgumentException("CategoryId must be valid when provided.", nameof(request));

                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                    throw new NotFoundException("CatalogCategory", "Category not found.");

                if (category.BusinessId != request.BusinessId)
                    throw new ArgumentException("Category does not belong to the same business.", nameof(request));
            }

            item.Name = request.Name.Trim();
            item.BasePrice = request.BasePrice;
            item.CategoryId = request.CategoryId;
            item.TrackStock = request.TrackStock;
            item.Type = request.Type;

            await _catalogRepository.UpdateAsync(item);
        }
    }
}
