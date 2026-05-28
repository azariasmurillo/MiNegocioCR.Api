using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Sales;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Sales;

/// <summary>
/// Integration tests against local PostgreSQL (MiNegocioCR_Dev) to catch schema/constraint issues
/// that InMemory provider does not enforce.
/// </summary>
public class RegisterSalePostgresIntegrationTests
{
    private static string? GetConnectionString()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine("..", "..", "..", "..", "appsettings.Development.json"), optional: true)
            .Build();
        return config.GetConnectionString("DefaultConnection");
    }

    private static AppDbContext CreatePgContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options);

    private static RegisterSaleUseCase CreateSut(AppDbContext ctx, IEnumerable<Payment>? repairPayments = null)
    {
        var inv = new Mock<IInventoryService>();
        inv.Setup(x => x.DecreaseStockAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var paymentSvc = new Mock<IPaymentService>();
        paymentSvc.Setup(x => x.GetPaymentsByRepairOrderAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(repairPayments?.ToList() ?? []);

        return new RegisterSaleUseCase(new SaleRepository(ctx), inv.Object, paymentSvc.Object, ctx);
    }

    [Fact]
    public async Task RepairSale_DiscountAndPrepaid_ZeroSaldo_PersistsOnPostgres()
    {
        var cs = GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            return; // skip silently if no config
        }

        await using var ctx = CreatePgContext(cs);
        try
        {
            await ctx.Database.CanConnectAsync();
        }
        catch
        {
            return; // skip if postgres not running
        }

        var bizId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity
        {
            Id = bizId,
            Name = "PG Integration Test",
            TaxRatePercent = 13m,
            CreatedAt = DateTime.UtcNow,
        });
        ctx.Contacts.Add(new Contact
        {
            Id = contactId,
            BusinessId = bizId,
            Name = "Cliente PG",
            Phone = "5068888" + Random.Shared.Next(1000, 9999),
            CreatedAt = DateTime.UtcNow,
        });
        var order = new RepairOrderEntity
        {
            Id = orderId,
            BusinessId = bizId,
            OrderNumber = "PG" + Random.Shared.Next(100000, 999999),
            Status = (int)RepairOrderStatus.Processed,
            ContactId = contactId,
            IsInvoiced = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        order.Items.Add(new RepairOrderItem
        {
            Id = itemId,
            RepairOrderId = orderId,
            Description = "Mantenimiento",
            Price = 35_000m,
            Quantity = 1,
        });
        ctx.RepairOrders.Add(order);
        ctx.Payments.Add(new Payment
        {
            Id = paymentId,
            BusinessId = bizId,
            RepairOrderId = orderId,
            Amount = 5_000m,
            Method = PaymentMethod.Cash,
            CreatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, ctx.Payments.Where(p => p.RepairOrderId == orderId).ToList());

        var act = () => sut.ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            RepairOrderId = orderId,
            Source = "Repair",
            DiscountKind = "FixedAmount",
            DiscountValue = 30_000m,
            Discount = 30_000m,
            PaymentMethods = [],
        });

        var result = await act.Should().NotThrowAsync();
        var saleId = result.Subject.GetType().GetProperty("Id")!.GetValue(result.Subject) as Guid?;
        saleId.Should().NotBeNull();

        var sale = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId!.Value);
        sale.TotalOrden.Should().Be(5_000m);
        sale.PrepaidAmount.Should().Be(5_000m);
        sale.Total.Should().Be(0m);

        // cleanup
        ctx.Sales.Remove(await ctx.Sales.Include(s => s.Items).Include(s => s.PaymentMethods)
            .FirstAsync(s => s.Id == saleId!.Value));
        var ro = await ctx.RepairOrders.FirstAsync(r => r.Id == orderId);
        ro.IsInvoiced = false;
        ro.SaleId = null;
        ro.Status = (int)RepairOrderStatus.Processed;
        ctx.Payments.RemoveRange(ctx.Payments.Where(p => p.Id == paymentId));
        ctx.RepairOrderItems.RemoveRange(ctx.RepairOrderItems.Where(i => i.RepairOrderId == orderId));
        ctx.RepairOrders.Remove(ro);
        ctx.Contacts.Remove(await ctx.Contacts.FirstAsync(c => c.Id == contactId));
        ctx.Businesses.Remove(await ctx.Businesses.FirstAsync(b => b.Id == bizId));
        await ctx.SaveChangesAsync();
    }
}
