using MiNegocioCR.Api.Application.UseCases.Variants;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Tests.UseCases.Variants;

public class UploadCatalogVariantImagesSortOrderTests
{
    [Fact]
    public void ResolveOccupiedSlots_maps_legacy_zero_sort_orders_to_slots()
    {
        var existing = new List<CatalogVariantImage>
        {
            new() { SortOrder = 0 },
            new() { SortOrder = 0 },
        };

        var occupied = UploadCatalogVariantImagesUseCase.ResolveOccupiedSlots(existing);

        Assert.Equal(new[] { 1, 2 }, occupied.OrderBy(x => x));
    }

    [Fact]
    public void TakeNextSortOrder_returns_first_free_slot()
    {
        var occupied = new HashSet<int> { 1, 3 };

        var next = UploadCatalogVariantImagesUseCase.TakeNextSortOrder(occupied);

        Assert.Equal(2, next);
        Assert.Equal(new[] { 1, 2, 3 }, occupied.OrderBy(x => x));
    }
}
