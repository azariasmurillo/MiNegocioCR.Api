using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class CreateVariantUseCaseTests
{
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly CreateVariantUseCase _sut;

    public CreateVariantUseCaseTests()
    {
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _sut = new CreateVariantUseCase(_variantRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_AddsVariantAndReturnsId()
    {
        var catalogItemId = Guid.NewGuid();
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(
            catalogItemId,
            sku: "SKU-001",
            price: 19.99m,
            initialStock: 50);

        result.Should().NotBeEmpty();
        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.Is<CatalogVariant>(v =>
                v.CatalogItemId == catalogItemId &&
                v.SKU == "SKU-001" &&
                v.Price == 19.99m &&
                v.StockQuantity == 50 &&
                v.IsActive)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroStock_CreatesVariantWithZeroStock()
    {
        var catalogItemId = Guid.NewGuid();
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(catalogItemId, "SKU-0", 0m, 0);

        result.Should().NotBeEmpty();
        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.Is<CatalogVariant>(v =>
                v.StockQuantity == 0 && v.Price == 0m)),
            Times.Once);
    }
}
