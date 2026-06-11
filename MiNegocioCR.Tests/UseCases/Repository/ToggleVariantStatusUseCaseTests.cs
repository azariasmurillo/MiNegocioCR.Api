using FluentAssertions;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class ToggleVariantStatusUseCaseTests
{
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly ToggleVariantStatusUseCase _sut;

    public ToggleVariantStatusUseCaseTests()
    {
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _sut = new ToggleVariantStatusUseCase(
            _variantRepositoryMock.Object,
            _inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActivating_SetsIsActiveTrue_WithoutInventoryMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = Guid.NewGuid(),
            StockQuantity = 0,
            IsActive = false,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);

        await _sut.ExecuteAsync(variantId, new ToggleVariantStatusRequestDto
        {
            BusinessId = businessId,
            IsActive = true,
        });

        variant.IsActive.Should().BeTrue();
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.IsAny<InventoryMovement>()),
            Times.Never);
        _variantRepositoryMock.Verify(x => x.UpdateAsync(variant), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeactivatingWithStock_ClearsStockAndRecordsMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = Guid.NewGuid(),
            StockQuantity = 7,
            IsActive = true,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);

        InventoryMovement? captured = null;
        _inventoryRepositoryMock
            .Setup(x => x.AddMovementAsync(It.IsAny<InventoryMovement>()))
            .Callback<InventoryMovement>(m => captured = m)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(variantId, new ToggleVariantStatusRequestDto
        {
            BusinessId = businessId,
            IsActive = false,
        });

        variant.IsActive.Should().BeFalse();
        variant.StockQuantity.Should().Be(0);
        captured.Should().NotBeNull();
        captured!.Quantity.Should().Be(-7);
        captured.Type.Should().Be(InventoryMovementType.Adjustment);
        captured.Notes.Should().Be("Presentación desactivada");
        captured.CatalogVariantId.Should().Be(variantId);
        captured.BusinessId.Should().Be(businessId);
        _variantRepositoryMock.Verify(x => x.UpdateAsync(variant), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDeactivatingWithZeroStock_DoesNotRecordMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = Guid.NewGuid(),
            StockQuantity = 0,
            IsActive = true,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);

        await _sut.ExecuteAsync(variantId, new ToggleVariantStatusRequestDto
        {
            BusinessId = businessId,
            IsActive = false,
        });

        variant.IsActive.Should().BeFalse();
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.IsAny<InventoryMovement>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVariantNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync((CatalogVariant?)null);

        var act = () => _sut.ExecuteAsync(variantId, new ToggleVariantStatusRequestDto
        {
            BusinessId = businessId,
            IsActive = false,
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBusinessIdEmpty_ThrowsArgumentException()
    {
        var act = () => _sut.ExecuteAsync(Guid.NewGuid(), new ToggleVariantStatusRequestDto
        {
            BusinessId = Guid.Empty,
            IsActive = true,
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request");
    }
}
