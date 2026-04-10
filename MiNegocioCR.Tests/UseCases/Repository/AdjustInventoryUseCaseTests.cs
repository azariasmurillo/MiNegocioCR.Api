using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class AdjustInventoryUseCaseTests
{
    private readonly Mock<IInventoryService> _inventoryServiceMock;

    public AdjustInventoryUseCaseTests()
    {
        _inventoryServiceMock = new Mock<IInventoryService>();
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
    public async Task ExecuteAsync_WithPositiveAdjustment_CallsAdjustStockAsync()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new AdjustInventoryUseCase(_inventoryServiceMock.Object, context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await sut.ExecuteAsync(businessId, variantId, 3, "Corrección por conteo");

        _inventoryServiceMock.Verify(
            x => x.AdjustStockAsync(businessId, variantId, 3, "Corrección por conteo"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeAdjustment_CallsAdjustStockAsync()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new AdjustInventoryUseCase(_inventoryServiceMock.Object, context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await sut.ExecuteAsync(businessId, variantId, -2, "Merma");

        _inventoryServiceMock.Verify(
            x => x.AdjustStockAsync(businessId, variantId, -2, "Merma"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAdjustmentIsZero_ThrowsArgumentException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new AdjustInventoryUseCase(_inventoryServiceMock.Object, context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var act = () => sut.ExecuteAsync(businessId, variantId, 0, "reason");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Adjustment must be a non-zero value*")
            .WithParameterName("adjustment");
        _inventoryServiceMock.Verify(
            x => x.AdjustStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVariantNotFound_PropagatesNotFoundException()
    {
        await using var context = (AppDbContext)CreateInMemoryAppContext();
        var sut = new AdjustInventoryUseCase(_inventoryServiceMock.Object, context);

        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        _inventoryServiceMock
            .Setup(x => x.AdjustStockAsync(businessId, variantId, 5, "reason"))
            .ThrowsAsync(new NotFoundException("CatalogVariant", "Variant not found"));

        var act = () => sut.ExecuteAsync(businessId, variantId, 5, "reason");

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "CatalogVariant");
    }
}
