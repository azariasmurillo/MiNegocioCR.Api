using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Repository;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Repository;

public class GetVariantBySkuUseCaseTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSkuMissing_ThrowsArgumentException()
    {
        await using var context = CreateContext();
        var sut = new GetVariantBySkuUseCase(new VariantRepository(context), context);

        var act = () => sut.ExecuteAsync(Guid.NewGuid(), "   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var sut = new GetVariantBySkuUseCase(new VariantRepository(context), context);

        var act = () => sut.ExecuteAsync(Guid.NewGuid(), "NO-EXISTE");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_FindsVariant_CaseInsensitive()
    {
        await using var context = CreateContext();
        var businessId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        context.Businesses.Add(new MiNegocioCR.Api.Domain.Entities.Business
        {
            Id = businessId,
            Name = "Test Shop",
            DefaultProfitMargin = 40m,
            TaxRatePercent = 13m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        context.CatalogItems.Add(new CatalogItem
        {
            Id = itemId,
            BusinessId = businessId,
            Name = "Mouse Logi",
            Type = CatalogItemType.Product,
            BasePrice = 5000m,
            TrackStock = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        var variant = new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = itemId,
            BusinessId = businessId,
            SKU = "ABC-123",
            Price = 8500m,
            CostPrice = 4000m,
            StockQuantity = 7,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        SkuNormalizer.Apply(variant, businessId, variant.SKU);
        context.CatalogVariants.Add(variant);
        context.CatalogVariantImages.Add(new CatalogVariantImage
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CatalogVariantId = variantId,
            ImageUrl = "https://cdn.example/main.webp",
            ThumbnailImageUrl = "https://cdn.example/thumb.webp",
            SortOrder = 1,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var sut = new GetVariantBySkuUseCase(new VariantRepository(context), context);
        var result = await sut.ExecuteAsync(businessId, "abc-123");

        result.VariantId.Should().Be(variantId);
        result.CatalogItemName.Should().Be("Mouse Logi");
        result.Sku.Should().Be("ABC-123");
        result.CurrentStock.Should().Be(7);
        result.ImageCount.Should().Be(1);
        result.PrimaryImageUrl.Should().Be("https://cdn.example/thumb.webp");
        result.EffectiveProfitMargin.Should().Be(40m);
    }
}
