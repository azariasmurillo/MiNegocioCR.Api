using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Sales;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Tests.UseCases.Sales;

public class RegisterSaleUseCaseTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static void SeedBusiness(AppDbContext ctx, Guid businessId, decimal taxRatePercent = 13m)
    {
        ctx.Businesses.Add(new BusinessEntity
        {
            Id = businessId,
            Name = "Test business",
            TaxRatePercent = taxRatePercent,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static void SeedProductVariant(AppDbContext ctx, Guid businessId, Guid variantId)
    {
        var itemId = Guid.NewGuid();
        ctx.CatalogItems.Add(new CatalogItem
        {
            Id = itemId,
            BusinessId = businessId,
            Name = "Test item",
            Type = CatalogItemType.Product,
            HasVariants = true,
            BasePrice = 0,
            TrackStock = true
        });
        ctx.CatalogVariants.Add(new CatalogVariant
        {
            Id = variantId,
            CatalogItemId = itemId,
            SKU = "T",
            Price = 10m,
            StockQuantity = 99,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static RegisterSaleUseCase CreateSut(AppDbContext ctx)
    {
        var inv = new Mock<IInventoryService>();
        inv.Setup(x => x.DecreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var payments = new Mock<IPaymentService>();
        payments.Setup(x => x.GetPaymentsByRepairOrderAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<Payment>());
        return new RegisterSaleUseCase(new SaleRepository(ctx), inv.Object, payments.Object, ctx);
    }

    private static Guid GetSaleId(object result)
    {
        var id = result.GetType().GetProperty("Id")?.GetValue(result);
        return id is Guid g ? g : Guid.Empty;
    }

    [Fact]
    public async Task ExecuteAsync_WithoutContactFields_CreatesSaleWithoutContact()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SeedBusiness(ctx, businessId);
        SeedProductVariant(ctx, businessId, variantId);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        var request = new CreateSaleRequestDto
        {
            BusinessId = businessId,
            Items =
            {
                new SaleItemRequestDto
                {
                    CatalogVariantId = variantId,
                    Quantity = 1,
                    UnitPrice = 5m,
                    ItemType = "Product"
                }
            }
        };

        var result = await sut.ExecuteAsync(request);
        var saleId = GetSaleId(result);

        saleId.Should().NotBeEmpty();
        var sale = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);
        sale.ContactId.Should().BeNull();
        sale.CustomerPhone.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithPhone_CreatesOrLinksContact()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SeedBusiness(ctx, businessId);
        SeedProductVariant(ctx, businessId, variantId);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        var request = new CreateSaleRequestDto
        {
            BusinessId = businessId,
            CustomerPhone = "50670001122",
            CustomerName = "Ana",
            CustomerEmail = "a@a.com",
            Items =
            {
                new SaleItemRequestDto
                {
                    CatalogVariantId = variantId,
                    Quantity = 1,
                    UnitPrice = 10m,
                    ItemType = "Product"
                }
            }
        };

        var result = await sut.ExecuteAsync(request);
        var saleId = GetSaleId(result);

        var sale = await ctx.Sales.AsNoTracking().Include(s => s.Contact).FirstAsync(s => s.Id == saleId);
        sale.ContactId.Should().NotBeNull();
        sale.CustomerPhone.Should().Be("50670001122");
        sale.Contact!.Name.Should().Be("Ana");
        sale.Contact.Phone.Should().Be("50670001122");
        sale.Contact.Email.Should().Be("a@a.com");
    }

    [Fact]
    public async Task ExecuteAsync_WhenContactExists_UpdatesNameAndEmail()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SeedBusiness(ctx, businessId);
        SeedProductVariant(ctx, businessId, variantId);
        var existing = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Viejo",
            Phone = "88889999",
            Email = "old@x.com",
            CreatedAt = DateTime.UtcNow
        };
        ctx.Contacts.Add(existing);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        var request = new CreateSaleRequestDto
        {
            BusinessId = businessId,
            CustomerPhone = "88889999",
            CustomerName = "Nuevo",
            CustomerEmail = "new@x.com",
            Items =
            {
                new SaleItemRequestDto
                {
                    CatalogVariantId = variantId,
                    Quantity = 1,
                    UnitPrice = 3m,
                    ItemType = "Product"
                }
            }
        };

        await sut.ExecuteAsync(request);

        var c = await ctx.Contacts.AsNoTracking().FirstAsync(x => x.Id == existing.Id);
        c.Name.Should().Be("Nuevo");
        c.Email.Should().Be("new@x.com");
    }

    [Fact]
    public async Task ExecuteAsync_WithNameAndEmailOnly_CreatesContactWithSyntheticPhone()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SeedBusiness(ctx, businessId);
        SeedProductVariant(ctx, businessId, variantId);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        var request = new CreateSaleRequestDto
        {
            BusinessId = businessId,
            CustomerName = "Sólo nombre",
            CustomerEmail = "m@e.com",
            Items =
            {
                new SaleItemRequestDto
                {
                    CatalogVariantId = variantId,
                    Quantity = 2,
                    UnitPrice = 1m,
                    ItemType = "Product"
                }
            }
        };

        var result = await sut.ExecuteAsync(request);
        var saleId = GetSaleId(result);

        var sale = await ctx.Sales.AsNoTracking().Include(s => s.Contact).FirstAsync(s => s.Id == saleId);
        sale.ContactId.Should().NotBeNull();
        sale.Contact!.Phone.Should().StartWith("SALE-ANON-");
        sale.CustomerPhone.Should().StartWith("SALE-ANON-");
        sale.Contact.Name.Should().Be("Sólo nombre");
    }
}
