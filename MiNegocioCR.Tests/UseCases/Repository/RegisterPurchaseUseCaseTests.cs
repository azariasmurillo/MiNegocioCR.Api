using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class RegisterPurchaseUseCaseTests
{
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<IPurchaseRepository> _purchaseRepositoryMock;

    public RegisterPurchaseUseCaseTests()
    {
        _inventoryServiceMock = new Mock<IInventoryService>();
        _purchaseRepositoryMock = new Mock<IPurchaseRepository>();
    }

    private static IAppDbContext CreateInMemoryAppContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsIncreaseStockAndPersistsPurchase()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await sut.ExecuteAsync(
            businessId,
            new List<(Guid VariantId, int Quantity, decimal Cost)>
            {
                (variantId, 5, 25m),
            });

        _inventoryServiceMock.Verify(
            x => x.IncreaseStockAsync(businessId, variantId, 5, "Purchase"),
            Times.Once);

        _purchaseRepositoryMock.Verify(
            x => x.AddPurchaseAsync(
                It.Is<Purchase>(
                    p => p.BusinessId == businessId
                         && p.Total == 125m
                         && p.Items.Count == 1
                         && p.Items.First().CatalogVariantId == variantId
                         && p.Items.First().Quantity == 5
                         && p.Items.First().Cost == 25m)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleLines_CallsIncreaseStockPerLine()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();

        await sut.ExecuteAsync(
            businessId,
            new List<(Guid VariantId, int Quantity, decimal Cost)>
            {
                (v1, 2, 10m),
                (v2, 3, 5m),
            });

        _inventoryServiceMock.Verify(
            x => x.IncreaseStockAsync(businessId, v1, 2, "Purchase"),
            Times.Once);
        _inventoryServiceMock.Verify(
            x => x.IncreaseStockAsync(businessId, v2, 3, "Purchase"),
            Times.Once);

        _purchaseRepositoryMock.Verify(
            x => x.AddPurchaseAsync(It.Is<Purchase>(p => p.Total == 35m && p.Items.Count == 2)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemsNull_ThrowsArgumentException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();

        var act = () => sut.ExecuteAsync(businessId, null!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("items");
        _purchaseRepositoryMock.Verify(
            x => x.AddPurchaseAsync(It.IsAny<Purchase>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemsEmpty_ThrowsArgumentException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();

        var act = () => sut.ExecuteAsync(businessId, new List<(Guid VariantId, int Quantity, decimal Cost)>());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("items");
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsZero_ThrowsArgumentException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => sut.ExecuteAsync(
            businessId,
            new List<(Guid VariantId, int Quantity, decimal Cost)> { (variantId, 0, 10m) });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Quantity must be greater than zero*")
            .WithParameterName("items");
        _inventoryServiceMock.Verify(
            x => x.IncreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuantityIsNegative_ThrowsArgumentException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => sut.ExecuteAsync(
            businessId,
            new List<(Guid VariantId, int Quantity, decimal Cost)> { (variantId, -1, 10m) });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("items");
        _inventoryServiceMock.Verify(
            x => x.IncreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCostIsNegative_ThrowsArgumentException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => sut.ExecuteAsync(
            businessId,
            new List<(Guid VariantId, int Quantity, decimal Cost)> { (variantId, 5, -1m) });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Cost must be greater than or equal to zero*")
            .WithParameterName("items");
    }

    [Fact]
    public async Task ExecuteAsync_WhenVariantNotFound_PropagatesNotFoundException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new RegisterPurchaseUseCase(
            _inventoryServiceMock.Object,
            _purchaseRepositoryMock.Object,
            context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        _inventoryServiceMock
            .Setup(x => x.IncreaseStockAsync(businessId, variantId, 5, "Purchase"))
            .ThrowsAsync(new NotFoundException("CatalogVariant", "Variant not found"));

        var act = () => sut.ExecuteAsync(
            businessId,
            new List<(Guid VariantId, int Quantity, decimal Cost)> { (variantId, 5, 25m) });

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogVariant" && ex.Message.Contains("Variant not found"));

        _purchaseRepositoryMock.Verify(
            x => x.AddPurchaseAsync(It.IsAny<Purchase>()),
            Times.Never);
    }
}
