using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class AdjustInventoryUseCaseTests
{
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly AdjustInventoryUseCase _sut;

    public AdjustInventoryUseCaseTests()
    {
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _sut = new AdjustInventoryUseCase(
            _variantRepositoryMock.Object,
            _inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithPositiveAdjustment_UpdatesVariantAndAddsMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            StockQuantity = 10
        };
        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);

        await _sut.ExecuteAsync(businessId, variantId, 3, "Corrección por conteo");

        variant.StockQuantity.Should().Be(13);
        _variantRepositoryMock.Verify(
            x => x.UpdateVariantAsync(It.Is<CatalogVariant>(v => v.StockQuantity == 13)),
            Times.Once);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.Quantity == 3 &&
                m.Notes == "Corrección por conteo" &&
                m.Type == MiNegocioCR.Api.Domain.Enums.InventoryMovementType.Adjustment)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeAdjustment_DecreasesStock()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant { Id = variantId, StockQuantity = 10 };
        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);

        await _sut.ExecuteAsync(businessId, variantId, -2, "Merma");

        variant.StockQuantity.Should().Be(8);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m => m.Quantity == -2)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAdjustmentIsZero_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => _sut.ExecuteAsync(businessId, variantId, 0, "reason");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Adjustment must be a non-zero value*")
            .WithParameterName("adjustment");
    }

    [Fact]
    public async Task ExecuteAsync_WhenVariantNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync((CatalogVariant?)null);

        var act = () => _sut.ExecuteAsync(businessId, variantId, 5, "reason");

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogVariant");
    }
}
