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

public class CreateVariantUseCaseTests
{
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ICatalogRepository> _catalogRepositoryMock;
    private readonly Mock<ICatalogOptionValueRepository> _optionValueRepositoryMock;
    private readonly Mock<ICatalogVariantOptionValueRepository> _variantOptionValueRepositoryMock;
    private readonly CreateVariantUseCase _sut;

    public CreateVariantUseCaseTests()
    {
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _catalogRepositoryMock = new Mock<ICatalogRepository>();
        _optionValueRepositoryMock = new Mock<ICatalogOptionValueRepository>();
        _variantOptionValueRepositoryMock = new Mock<ICatalogVariantOptionValueRepository>();
        _variantOptionValueRepositoryMock
            .Setup(x => x.ExistsVariantWithSameOptionValueCombinationAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(false);
        _variantRepositoryMock
            .Setup(x => x.ExistsSkuForCatalogItemAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(false);
        _sut = new CreateVariantUseCase(
            _variantRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _catalogRepositoryMock.Object,
            _optionValueRepositoryMock.Object,
            _variantOptionValueRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithCostAndMargin_ComputesPrice_WhenNotManual()
    {
        var businessId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = businessId });
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU-COST",
            CostPrice = 100m,
            ProfitMargin = 25m,
            Price = 1m,
            SetPriceManually = false,
            InitialStock = 0
        };

        await _sut.ExecuteAsync(request);

        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.Is<CatalogVariant>(v =>
                v.Price == 125m &&
                v.CostPrice == 100m &&
                v.ProfitMargin == 25m)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCostAndMargin_NormalizesPrice_ToNearestFiveColonesCeiling()
    {
        var businessId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = businessId });
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU-CRC",
            CostPrice = 10071.68m,
            ProfitMargin = 25m,
            Price = 0m,
            SetPriceManually = false,
            InitialStock = 0
        };

        await _sut.ExecuteAsync(request);

        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.Is<CatalogVariant>(v =>
                v.Price == 12590m)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_AddsVariantAndReturnsId()
    {
        var businessId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = businessId });
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU-001",
            Price = 19.99m,
            InitialStock = 50
        };

        var result = await _sut.ExecuteAsync(request);

        result.Should().NotBeEmpty();
        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.Is<CatalogVariant>(v =>
                v.CatalogItemId == catalogItemId &&
                v.SKU == "SKU-001" &&
                v.Price == 20m &&
                v.StockQuantity == 50 &&
                v.IsActive)),
            Times.Once);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.Is<InventoryMovement>(m =>
                m.BusinessId == businessId &&
                m.Quantity == 50 &&
                m.Type == InventoryMovementType.Purchase &&
                m.Notes == "Initial stock")),
            Times.Once);
        _variantOptionValueRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IReadOnlyList<CatalogVariantOptionValue>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroStock_CreatesVariantWithZeroStock_AndNoMovement()
    {
        var catalogItemId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = Guid.NewGuid() });
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU-0",
            Price = 0m,
            InitialStock = 0
        };

        var result = await _sut.ExecuteAsync(request);

        result.Should().NotBeEmpty();
        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.Is<CatalogVariant>(v =>
                v.StockQuantity == 0 && v.Price == 0m)),
            Times.Once);
        _inventoryRepositoryMock.Verify(
            x => x.AddMovementAsync(It.IsAny<InventoryMovement>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCatalogItemNotFound_ThrowsNotFoundException()
    {
        var catalogItemId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync((CatalogItem?)null);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU",
            Price = 1m,
            InitialStock = 0
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogItem");
        _variantRepositoryMock.Verify(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionValues_PersistsLinks_AfterVariant()
    {
        var businessId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var colorValueId = Guid.NewGuid();
        var sizeValueId = Guid.NewGuid();

        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = businessId });
        _optionValueRepositoryMock
            .Setup(x => x.GetByIdsWithCatalogOptionAsync(It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(new List<CatalogOptionValue>
            {
                new()
                {
                    Id = colorValueId,
                    CatalogOptionId = Guid.NewGuid(),
                    Value = "Negro",
                    CatalogOption = new CatalogOption
                    {
                        Id = Guid.NewGuid(),
                        CatalogItemId = catalogItemId,
                        Name = "Color"
                    }
                },
                new()
                {
                    Id = sizeValueId,
                    CatalogOptionId = Guid.NewGuid(),
                    Value = "16GB",
                    CatalogOption = new CatalogOption
                    {
                        Id = Guid.NewGuid(),
                        CatalogItemId = catalogItemId,
                        Name = "Tamaño"
                    }
                }
            });
        _variantOptionValueRepositoryMock
            .Setup(x => x.ExistsVariantWithSameOptionValueCombinationAsync(catalogItemId, It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(false);
        _variantRepositoryMock
            .Setup(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()))
            .Returns(Task.CompletedTask);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU-MIX",
            Price = 10m,
            InitialStock = 0,
            OptionValueIds = new List<Guid> { colorValueId, sizeValueId }
        };

        await _sut.ExecuteAsync(request);

        _variantRepositoryMock.Verify(
            x => x.AddVariantAsync(It.IsAny<CatalogVariant>()),
            Times.Once);
        _variantOptionValueRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IReadOnlyList<CatalogVariantOptionValue>>(list =>
                list.Count == 2 &&
                list.All(l => l.CatalogOptionValueId == colorValueId || l.CatalogOptionValueId == sizeValueId))),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateOptionValueIds_ThrowsArgumentException()
    {
        var catalogItemId = Guid.NewGuid();
        var dupId = Guid.NewGuid();
        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = Guid.NewGuid() });

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "X",
            Price = 1m,
            InitialStock = 0,
            OptionValueIds = new List<Guid> { dupId, dupId }
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
        _variantRepositoryMock.Verify(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionValueBelongsToAnotherItem_ThrowsArgumentException()
    {
        var catalogItemId = Guid.NewGuid();
        var otherItemId = Guid.NewGuid();
        var valueId = Guid.NewGuid();

        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = Guid.NewGuid() });
        _optionValueRepositoryMock
            .Setup(x => x.GetByIdsWithCatalogOptionAsync(It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(new List<CatalogOptionValue>
            {
                new()
                {
                    Id = valueId,
                    CatalogOptionId = Guid.NewGuid(),
                    Value = "X",
                    CatalogOption = new CatalogOption
                    {
                        Id = Guid.NewGuid(),
                        CatalogItemId = otherItemId,
                        Name = "Opt"
                    }
                }
            });

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU",
            Price = 1m,
            InitialStock = 0,
            OptionValueIds = new List<Guid> { valueId }
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .Where(ex => ex.Message.Contains("same catalog item", StringComparison.Ordinal));
        _variantRepositoryMock.Verify(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCombinationAlreadyExists_ThrowsArgumentException()
    {
        var catalogItemId = Guid.NewGuid();
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();

        _catalogRepositoryMock
            .Setup(x => x.GetItemByIdAsync(catalogItemId))
            .ReturnsAsync(new CatalogItem { Id = catalogItemId, BusinessId = Guid.NewGuid() });
        _optionValueRepositoryMock
            .Setup(x => x.GetByIdsWithCatalogOptionAsync(It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(new List<CatalogOptionValue>
            {
                new()
                {
                    Id = v1,
                    CatalogOption = new CatalogOption { CatalogItemId = catalogItemId }
                },
                new()
                {
                    Id = v2,
                    CatalogOption = new CatalogOption { CatalogItemId = catalogItemId }
                }
            });
        _variantOptionValueRepositoryMock
            .Setup(x => x.ExistsVariantWithSameOptionValueCombinationAsync(catalogItemId, It.IsAny<IReadOnlyList<Guid>>()))
            .ReturnsAsync(true);

        var request = new CreateVariantRequestDto
        {
            CatalogItemId = catalogItemId,
            SKU = "SKU",
            Price = 1m,
            InitialStock = 0,
            OptionValueIds = new List<Guid> { v1, v2 }
        };

        var act = () => _sut.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .Where(ex => ex.Message.Contains("combination already exists", StringComparison.Ordinal));
        _variantRepositoryMock.Verify(x => x.AddVariantAsync(It.IsAny<CatalogVariant>()), Times.Never);
    }
}
