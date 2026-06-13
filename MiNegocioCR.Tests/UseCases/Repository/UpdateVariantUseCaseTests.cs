using FluentAssertions;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Domain.Exceptions;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class UpdateVariantUseCaseTests
{
    private readonly Mock<IVariantRepository> _variantRepositoryMock;
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly UpdateVariantUseCase _sut;

    public UpdateVariantUseCaseTests()
    {
        _variantRepositoryMock = new Mock<IVariantRepository>();
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _sut = new UpdateVariantUseCase(
            _variantRepositoryMock.Object,
            _businessRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSetProfitMargin_PersistsMarginAndRecalculatesPrice()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = catalogItemId,
            SKU = "SKU-1",
            CostPrice = 100m,
            Price = 100m,
            IsActive = true,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);
        _variantRepositoryMock
            .Setup(x => x.ExistsSkuForBusinessAsync(businessId, It.IsAny<string>(), variantId))
            .ReturnsAsync(false);
        _businessRepositoryMock
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(new BusinessEntity { Id = businessId, TaxRatePercent = 13m });

        await _sut.ExecuteAsync(variantId, new UpdateVariantRequestDto
        {
            BusinessId = businessId,
            SKU = "SKU-1",
            CostPrice = 100m,
            ProfitMargin = 50m,
            SetProfitMargin = true,
            SetPriceManually = false,
            Price = 0m,
        });

        variant.ProfitMargin.Should().Be(50m);
        variant.Price.Should().Be(170m);
        _variantRepositoryMock.Verify(x => x.UpdateAsync(variant), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSetProfitMarginNull_ClearsOverride()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = catalogItemId,
            SKU = "SKU-1",
            CostPrice = 100m,
            Price = 145m,
            ProfitMargin = 25m,
            IsActive = true,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);
        _variantRepositoryMock
            .Setup(x => x.ExistsSkuForBusinessAsync(businessId, It.IsAny<string>(), variantId))
            .ReturnsAsync(false);
        _businessRepositoryMock
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(new BusinessEntity { Id = businessId, TaxRatePercent = 13m });

        await _sut.ExecuteAsync(variantId, new UpdateVariantRequestDto
        {
            BusinessId = businessId,
            SKU = "SKU-1",
            CostPrice = 100m,
            ProfitMargin = null,
            SetProfitMargin = true,
            SetPriceManually = true,
            Price = 200m,
        });

        variant.ProfitMargin.Should().BeNull();
        variant.Price.Should().Be(200m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManualPrice_UsesProvidedPrice()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = catalogItemId,
            SKU = "SKU-1",
            CostPrice = 50m,
            Price = 100m,
            IsActive = true,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);
        _variantRepositoryMock
            .Setup(x => x.ExistsSkuForBusinessAsync(businessId, It.IsAny<string>(), variantId))
            .ReturnsAsync(false);
        _businessRepositoryMock
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(new BusinessEntity { Id = businessId, TaxRatePercent = 13m });

        await _sut.ExecuteAsync(variantId, new UpdateVariantRequestDto
        {
            BusinessId = businessId,
            SKU = "SKU-UPD",
            CostPrice = 50m,
            SetPriceManually = true,
            Price = 29750m,
        });

        variant.SKU.Should().Be("SKU-UPD");
        variant.Price.Should().Be(29750m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDuplicateSku_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = catalogItemId,
            SKU = "SKU-1",
            CostPrice = 10m,
            Price = 20m,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);
        _variantRepositoryMock
            .Setup(x => x.ExistsSkuForBusinessAsync(businessId, "DUP", variantId))
            .ReturnsAsync(true);

        var act = () => _sut.ExecuteAsync(variantId, new UpdateVariantRequestDto
        {
            BusinessId = businessId,
            SKU = "DUP",
            CostPrice = 10m,
            SetPriceManually = true,
            Price = 20m,
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("SKU");
    }

    [Fact]
    public async Task ExecuteAsync_WhenVariantNotFound_ThrowsNotFoundException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync((CatalogVariant?)null);

        var act = () => _sut.ExecuteAsync(variantId, new UpdateVariantRequestDto
        {
            BusinessId = businessId,
            SKU = "X",
            CostPrice = 1m,
            SetPriceManually = true,
            Price = 10m,
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNegativeProfitMargin_ThrowsArgumentException()
    {
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var catalogItemId = Guid.NewGuid();
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = catalogItemId,
            SKU = "SKU-1",
            CostPrice = 10m,
            Price = 20m,
        };

        _variantRepositoryMock
            .Setup(x => x.GetVariantAsync(variantId, businessId))
            .ReturnsAsync(variant);
        _variantRepositoryMock
            .Setup(x => x.ExistsSkuForBusinessAsync(businessId, It.IsAny<string>(), variantId))
            .ReturnsAsync(false);

        var act = () => _sut.ExecuteAsync(variantId, new UpdateVariantRequestDto
        {
            BusinessId = businessId,
            SKU = "SKU-1",
            CostPrice = 10m,
            SetProfitMargin = true,
            ProfitMargin = -1m,
            SetPriceManually = false,
            Price = 0m,
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request");
    }
}
