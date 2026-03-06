using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Enums;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class CreateCatalogItemUseCaseTests
{
    private readonly Mock<ICatalogRepository> _catalogRepositoryMock;
    private readonly CreateCatalogItemUseCase _sut;

    public CreateCatalogItemUseCaseTests()
    {
        _catalogRepositoryMock = new Mock<ICatalogRepository>();
        _sut = new CreateCatalogItemUseCase(_catalogRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyName_CreatesItemAndReturnsId()
    {
        var businessId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.AddItemAsync(It.IsAny<MiNegocioCR.Api.Domain.Entities.CatalogItem>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(
            businessId,
            name: "",
            basePrice: 9.99m,
            trackStock: true,
            type: CatalogItemType.Product);

        result.Should().NotBeEmpty();
        _catalogRepositoryMock.Verify(
            x => x.AddItemAsync(It.Is<MiNegocioCR.Api.Domain.Entities.CatalogItem>(i =>
                i.BusinessId == businessId &&
                i.BasePrice == 9.99m &&
                i.TrackStock &&
                i.Type == CatalogItemType.Product)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullName_CreatesItemAndReturnsId()
    {
        var businessId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.AddItemAsync(It.IsAny<MiNegocioCR.Api.Domain.Entities.CatalogItem>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ExecuteAsync(
            businessId,
            name: null!,
            basePrice: 5m,
            trackStock: false,
            type: CatalogItemType.Service);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNameIsNotEmpty_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();

        var act = () => _sut.ExecuteAsync(
            businessId,
            name: "Producto válido",
            basePrice: 10m,
            trackStock: true,
            type: CatalogItemType.Product);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Name cannot be null or empty*")
            .WithParameterName("name");
    }
}
