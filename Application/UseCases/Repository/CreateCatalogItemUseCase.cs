using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class CreateCatalogItemUseCase : ICreateCatalogItemUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public CreateCatalogItemUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<Guid> ExecuteAsync(
            Guid businessId,
            string name,
            decimal basePrice,
            bool trackStock,
            CatalogItemType type)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));

            var item = new CatalogItem
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = name,
                BasePrice = basePrice,
                TrackStock = trackStock,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            await _catalogRepository.AddItemAsync(item);

            return item.Id;
        }
    }
}
