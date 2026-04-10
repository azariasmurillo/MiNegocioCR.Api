using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Infrastructure.Services;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.Services.Inventory;

public class InventoryServiceTests
{
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ILowStockAlertService> _alertServiceMock;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _alertServiceMock = new Mock<ILowStockAlertService>();
        _sut = new InventoryService(
            _variantRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _alertServiceMock.Object);
    }

    [Fact]
    public async Task IncreaseStockAsync_WithValidInput_UpdatesVariantAndAddsMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 10 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        await _sut.IncreaseStockAsync(businessId, variantId, 5, "Purchase");

        variant.StockQuantity.Should().Be(15);
        _variantRepositoryMock.Verify(x => x.UpdateVariantAsync(It.Is<CatalogVariant>(v => v.StockQuantity == 15)), Times.Once);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.Quantity == 5 && m.Type == MiNegocioCR.Api.Domain.Enums.InventoryMovementType.Purchase)),
            Times.Once);
    }

    [Fact]
    public async Task IncreaseStockAsync_WhenVariantNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync((CatalogVariant?)null);

        var act = () => _sut.IncreaseStockAsync(businessId, variantId, 5, "ref");

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogVariant");
    }

    [Fact]
    public async Task IncreaseStockAsync_WhenQuantityZeroOrNegative_ThrowsArgumentException()
    {
        var variant = new CatalogVariant { Id = Guid.NewGuid(), StockQuantity = 10 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(variant);

        await ((Func<Task>)(() => _sut.IncreaseStockAsync(Guid.NewGuid(), variant.Id, 0, "ref")))
            .Should().ThrowAsync<ArgumentException>();
        await ((Func<Task>)(() => _sut.IncreaseStockAsync(Guid.NewGuid(), variant.Id, -1, "ref")))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DecreaseStockAsync_WithValidInput_UpdatesVariantAndAddsMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 10, LowStockThreshold = 2 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        await _sut.DecreaseStockAsync(businessId, variantId, 3, "Sale");

        variant.StockQuantity.Should().Be(7);
        _variantRepositoryMock.Verify(x => x.UpdateVariantAsync(It.Is<CatalogVariant>(v => v.StockQuantity == 7)), Times.Once);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.Quantity == -3 && m.Type == MiNegocioCR.Api.Domain.Enums.InventoryMovementType.Sale)),
            Times.Once);
    }

    [Fact]
    public async Task DecreaseStockAsync_WhenNotEnoughStock_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 2 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        var act = () => _sut.DecreaseStockAsync(businessId, variantId, 5, "Sale");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Not enough stock*");
    }

    [Fact]
    public async Task DecreaseStockAsync_WhenStockAtOrBelowLowThreshold_CallsNotifyLowStock()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 5, LowStockThreshold = 5 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        await _sut.DecreaseStockAsync(businessId, variantId, 1, "Sale");

        _alertServiceMock.Verify(
            x => x.NotifyLowStock(businessId, It.Is<CatalogVariant>(v => v.StockQuantity == 4)),
            Times.Once);
    }

    [Fact]
    public async Task AdjustStockAsync_WithValidInput_UpdatesVariantAndAddsMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 10 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        await _sut.AdjustStockAsync(businessId, variantId, 4, "Ajuste conteo");

        variant.StockQuantity.Should().Be(14);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.Quantity == 4 && m.Notes == "Ajuste conteo")),
            Times.Once);
    }

    [Fact]
    public async Task AdjustStockAsync_WhenQuantityZero_ThrowsArgumentException()
    {
        var variant = new CatalogVariant { Id = Guid.NewGuid(), StockQuantity = 10 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(variant);

        var act = () => _sut.AdjustStockAsync(Guid.NewGuid(), variant.Id, 0, "reason");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public async Task AdjustStockAsync_WithNegativeQuantity_DecreasesStockAndRecordsSignedQuantity()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 10 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        await _sut.AdjustStockAsync(businessId, variantId, -3, "Merma");

        variant.StockQuantity.Should().Be(7);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.Quantity == -3 &&
                m.Type == MiNegocioCR.Api.Domain.Enums.InventoryMovementType.Adjustment &&
                m.Notes == "Merma")),
            Times.Once);
    }

    [Fact]
    public async Task AdjustStockAsync_WhenNegativeExceedsStock_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 2 };
        _variantRepositoryMock.Setup(x => x.GetVariantAsync(variantId, businessId)).ReturnsAsync(variant);

        var act = () => _sut.AdjustStockAsync(businessId, variantId, -5, "reason");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Not enough stock*");
    }
}
