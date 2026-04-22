using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Sales;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;

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

    [Fact]
    public async Task ExecuteAsync_WithoutContactFields_CreatesSaleWithoutContact()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var inv = new Mock<IInventoryService>();
        inv.Setup(x => x.DecreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var sut = new RegisterSaleUseCase(new SaleRepository(ctx), inv.Object, ctx);

        var id = await sut.ExecuteAsync(
            businessId,
            new List<(Guid, int, decimal)> { (variantId, 1, 5m) });

        id.Should().NotBeEmpty();
        var sale = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == id);
        sale.ContactId.Should().BeNull();
        sale.CustomerPhone.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithPhone_CreatesOrLinksContact()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var inv = new Mock<IInventoryService>();
        inv.Setup(x => x.DecreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var sut = new RegisterSaleUseCase(new SaleRepository(ctx), inv.Object, ctx);

        var id = await sut.ExecuteAsync(
            businessId,
            new List<(Guid, int, decimal)> { (variantId, 1, 10m) },
            customerPhone: "50670001122",
            customerName: "Ana",
            customerEmail: "a@a.com");

        var sale = await ctx.Sales.AsNoTracking().Include(s => s.Contact).FirstAsync(s => s.Id == id);
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

        var inv = new Mock<IInventoryService>();
        inv.Setup(x => x.DecreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var sut = new RegisterSaleUseCase(new SaleRepository(ctx), inv.Object, ctx);

        await sut.ExecuteAsync(
            businessId,
            new List<(Guid, int, decimal)> { (variantId, 1, 3m) },
            customerPhone: "88889999",
            customerName: "Nuevo",
            customerEmail: "new@x.com");

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
        var inv = new Mock<IInventoryService>();
        inv.Setup(x => x.DecreaseStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var sut = new RegisterSaleUseCase(new SaleRepository(ctx), inv.Object, ctx);

        var id = await sut.ExecuteAsync(
            businessId,
            new List<(Guid, int, decimal)> { (variantId, 2, 1m) },
            customerName: "Sólo nombre",
            customerEmail: "m@e.com");

        var sale = await ctx.Sales.AsNoTracking().Include(s => s.Contact).FirstAsync(s => s.Id == id);
        sale.ContactId.Should().NotBeNull();
        sale.Contact!.Phone.Should().StartWith("SALE-ANON-");
        sale.CustomerPhone.Should().StartWith("SALE-ANON-");
        sale.Contact.Name.Should().Be("Sólo nombre");
    }
}
