using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Repository;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class RegisterSaleUseCaseTests
{
    private readonly Mock<ISaleRepository> _saleRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly RegisterSaleUseCase _sut;

    public RegisterSaleUseCaseTests()
    {
        _saleRepositoryMock = new Mock<ISaleRepository>();
        _inventoryServiceMock = new Mock<IInventoryService>();
        _sut = new RegisterSaleUseCase(_saleRepositoryMock.Object, _inventoryServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidItems_ReturnsSaleIdAndCallsDependencies()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var items = new List<(Guid variantId, int quantity, decimal price)>
        {
            (variantId, 2, 10.50m)
        };

        var result = await _sut.ExecuteAsync(businessId, items);

        result.Should().NotBeEmpty();
        _inventoryServiceMock.Verify(
            x => x.DecreaseStockAsync(businessId, variantId, 2, "Sale"),
            Times.Once);
        _saleRepositoryMock.Verify(
            x => x.AddSaleAsync(It.Is<MiNegocioCR.Api.Domain.Entities.Sale>(s =>
                s.BusinessId == businessId &&
                s.Items.Count == 1 &&
                s.Items.First().CatalogVariantId == variantId &&
                s.Items.First().Quantity == 2 &&
                s.Items.First().Price == 10.50m &&
                s.Total == 21.00m)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleItems_CalculatesTotalAndDecreasesStockForEach()
    {
        var businessId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();
        var items = new List<(Guid variantId, int quantity, decimal price)>
        {
            (variant1, 1, 5m),
            (variant2, 3, 10m)
        };

        var result = await _sut.ExecuteAsync(businessId, items);

        result.Should().NotBeEmpty();
        _inventoryServiceMock.Verify(
            x => x.DecreaseStockAsync(businessId, variant1, 1, "Sale"),
            Times.Once);
        _inventoryServiceMock.Verify(
            x => x.DecreaseStockAsync(businessId, variant2, 3, "Sale"),
            Times.Once);
        _saleRepositoryMock.Verify(
            x => x.AddSaleAsync(It.Is<MiNegocioCR.Api.Domain.Entities.Sale>(s => s.Total == 35m)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemsIsNull_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();

        var act = () => _sut.ExecuteAsync(businessId, null!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one item is required*")
            .WithParameterName("items");
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemsIsEmpty_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var items = new List<(Guid variantId, int quantity, decimal price)>();

        var act = () => _sut.ExecuteAsync(businessId, items);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one item is required*")
            .WithParameterName("items");
    }
}
