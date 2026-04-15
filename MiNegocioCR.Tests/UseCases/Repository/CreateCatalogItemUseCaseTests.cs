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

public class CreateCatalogItemUseCaseTests
{
    private readonly Mock<ICatalogRepository> _catalogRepositoryMock;
    private readonly Mock<ICatalogCategoryRepository> _categoryRepositoryMock;
    private readonly CreateCatalogItemUseCase _sut;

    public CreateCatalogItemUseCaseTests()
    {
        _catalogRepositoryMock = new Mock<ICatalogRepository>();
        _categoryRepositoryMock = new Mock<ICatalogCategoryRepository>();
        _sut = new CreateCatalogItemUseCase(
            _catalogRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidName_CreatesItemAndReturnsId()
    {
        var businessId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.AddItemAsync(It.IsAny<CatalogItem>()))
            .Returns(Task.CompletedTask);

        var request = new CreateCatalogItemRequestDto
        {
            BusinessId = businessId,
            Name = "Producto",
            BasePrice = 9.99m,
            TrackStock = true,
            Type = CatalogItemType.Product
        };

        var result = await _sut.ExecuteAsync(request);

        result.Should().NotBeEmpty();
        _catalogRepositoryMock.Verify(
            x => x.AddItemAsync(It.Is<CatalogItem>(i =>
                i.BusinessId == businessId &&
                i.Name == "Producto" &&
                i.BasePrice == 9.99m &&
                i.TrackStock &&
                i.Type == CatalogItemType.Product &&
                i.CategoryId == null)),
            Times.Once);
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryId_AssignsWhenSameBusiness()
    {
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(new CatalogCategory { Id = categoryId, BusinessId = businessId, Name = "Cat" });
        _catalogRepositoryMock
            .Setup(x => x.AddItemAsync(It.IsAny<CatalogItem>()))
            .Returns(Task.CompletedTask);

        var request = new CreateCatalogItemRequestDto
        {
            BusinessId = businessId,
            Name = "Producto",
            BasePrice = 1m,
            TrackStock = false,
            Type = CatalogItemType.Product,
            CategoryId = categoryId
        };

        await _sut.ExecuteAsync(request);

        _catalogRepositoryMock.Verify(
            x => x.AddItemAsync(It.Is<CatalogItem>(i => i.CategoryId == categoryId)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryNotFound_ThrowsNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync((CatalogCategory?)null);

        var request = new CreateCatalogItemRequestDto
        {
            BusinessId = Guid.NewGuid(),
            Name = "X",
            BasePrice = 1m,
            TrackStock = false,
            Type = CatalogItemType.Product,
            CategoryId = categoryId
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogCategory");
        _catalogRepositoryMock.Verify(x => x.AddItemAsync(It.IsAny<CatalogItem>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoryBelongsToOtherBusiness_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(new CatalogCategory { Id = categoryId, BusinessId = Guid.NewGuid(), Name = "Cat" });

        var request = new CreateCatalogItemRequestDto
        {
            BusinessId = businessId,
            Name = "X",
            BasePrice = 1m,
            TrackStock = false,
            Type = CatalogItemType.Product,
            CategoryId = categoryId
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .Where(ex => ex.Message.Contains("same business", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyName_ThrowsArgumentException()
    {
        var request = new CreateCatalogItemRequestDto
        {
            BusinessId = Guid.NewGuid(),
            Name = "",
            BasePrice = 9.99m,
            TrackStock = true,
            Type = CatalogItemType.Product
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Name cannot be null or empty*")
            .WithParameterName("request");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullName_ThrowsArgumentException()
    {
        var request = new CreateCatalogItemRequestDto
        {
            BusinessId = Guid.NewGuid(),
            Name = null,
            BasePrice = 5m,
            TrackStock = false,
            Type = CatalogItemType.Service
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Name cannot be null or empty*")
            .WithParameterName("request");
    }
}
