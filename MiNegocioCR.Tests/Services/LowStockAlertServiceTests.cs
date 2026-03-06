using FluentAssertions;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Services;
using Xunit;

namespace MiNegocioCR.Tests.Services;

public class LowStockAlertServiceTests
{
    private readonly LowStockAlertService _sut = new();

    [Fact]
    public async Task NotifyLowStock_WithValidVariant_CompletesWithoutThrowing()
    {
        var businessId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = Guid.NewGuid(),
            SKU = "SKU-001",
            StockQuantity = 1,
            LowStockThreshold = 5
        };

        var act = () => _sut.NotifyLowStock(businessId, variant);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyLowStock_CanBeCalledMultipleTimes_CompletesSuccessfully()
    {
        var variant = new CatalogVariant { SKU = "X", StockQuantity = 0 };

        await _sut.NotifyLowStock(Guid.NewGuid(), variant);
        await _sut.NotifyLowStock(Guid.NewGuid(), variant);
    }
}
