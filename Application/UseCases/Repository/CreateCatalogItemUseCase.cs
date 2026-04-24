using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class CreateCatalogItemUseCase : ICreateCatalogItemUseCase
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICatalogCategoryRepository _categoryRepository;

        public CreateCatalogItemUseCase(
            ICatalogRepository catalogRepository,
            ICatalogCategoryRepository categoryRepository)
        {            
            _catalogRepository = catalogRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateCatalogItemRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(request));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            Guid? categoryId = request.CategoryId;

            if (categoryId.HasValue && categoryId.Value == Guid.Empty)
                throw new ArgumentException("CategoryId must be a valid id when provided.", nameof(request));

            if (categoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId.Value);
                if (category == null)
                    throw new NotFoundException("CatalogCategory", "Category not found.");

                if (category.BusinessId != request.BusinessId)
                {
                    throw new ArgumentException(
                        "Category does not belong to the same business.",
                        nameof(request));
                }
            }

            var item = new CatalogItem
            {
                Id = Guid.NewGuid(),
                BusinessId = request.BusinessId,
                CategoryId = categoryId,
                Name = request.Name,
                BasePrice = request.BasePrice,
                TrackStock = request.TrackStock,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow
            };

            await _catalogRepository.AddItemAsync(item);

            return item.Id;
        }
    }
}
