using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class RegisterPurchaseUseCaseTests
{
    private readonly Mock<IPurchaseRepository> _purchaseRepositoryMock;
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly RegisterPurchaseUseCase _sut;

    public RegisterPurchaseUseCaseTests()
    {
        _purchaseRepositoryMock = new Mock<IPurchaseRepository>();
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _sut = new RegisterPurchaseUseCase(
            _purchaseRepositoryMock.Object,
            _variantRepositoryMock.Object,
            _inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_UpdatesVariantAndAddsMovement()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = Guid.NewGuid(),
            StockQuantity = 10,
            Price = 5m
        };
        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);

        await _sut.ExecuteAsync(businessId, variantId, quantity: 5, cost: 25m);

        variant.StockQuantity.Should().Be(15);
        _variantRepositoryMock.Verify(
            x => x.UpdateVariantAsync(It.Is<CatalogVariant>(v => v.StockQuantity == 15)),
            Times.Once);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.BusinessId == businessId &&
                m.CatalogVariantId == variantId &&
                m.Quantity == 5 &&
                m.Type == MiNegocioCR.Api.Domain.Enums.InventoryMovementType.Purchase)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsZero_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => _sut.ExecuteAsync(businessId, variantId, 0, 10m);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Quantity must be greater than zero*")
            .WithParameterName("quantity");
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsNegative_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => _sut.ExecuteAsync(businessId, variantId, -1, 10m);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public async Task ExecuteAsync_WhenVariantNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync((CatalogVariant?)null);

        var act = () => _sut.ExecuteAsync(businessId, variantId, 5, 25m);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogVariant" && ex.Message.Contains("Variant not found"));
    }
}
