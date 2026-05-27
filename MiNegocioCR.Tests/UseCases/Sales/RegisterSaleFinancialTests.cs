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
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;

namespace MiNegocioCR.Tests.UseCases.Sales;

/// <summary>
/// Tests para la lógica financiera del refactor Mayo 2026:
/// - Separación correcta de descuento real vs prepagos
/// - TotalOrden / PrepaidAmount / saldoPendiente
/// - TotalProfit basado en TotalOrden (no saldo parcial)
/// - SalePaymentMethods guardados correctamente
/// - Validaciones de estado de orden de reparación
/// - ParsePaymentMethod con soporte español y rechazo de valores inválidos
/// </summary>
public class RegisterSaleFinancialTests
{
    // ── Helpers de setup ──────────────────────────────────────────────────────

    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private static RegisterSaleUseCase CreateSut(
        AppDbContext ctx,
        IEnumerable<Payment>? repairPayments = null)
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

    private static void SeedBusiness(AppDbContext ctx, Guid businessId, decimal taxRate = 13m)
    {
        ctx.Businesses.Add(new BusinessEntity
        {
            Id        = businessId,
            Name      = "Taller Test",
            TaxRatePercent = taxRate,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static Guid SeedVariant(AppDbContext ctx, Guid businessId, decimal price = 1000m)
    {
        var itemId    = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        ctx.CatalogItems.Add(new CatalogItem
        {
            Id         = itemId,
            BusinessId = businessId,
            Name       = "Pantalla",
            Type       = CatalogItemType.Product,
            HasVariants = true,
            BasePrice  = 0,
            TrackStock = true
        });
        ctx.CatalogVariants.Add(new CatalogVariant
        {
            Id            = variantId,
            CatalogItemId = itemId,
            SKU           = "PANT-001",
            Price         = price,
            StockQuantity = 50,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow
        });
        return variantId;
    }

    private static RepairOrderEntity SeedRepairOrder(
        AppDbContext ctx,
        Guid businessId,
        decimal itemPrice      = 50_000m,
        int    itemQty         = 1,
        bool   isInvoiced      = false,
        RepairOrderStatus status = RepairOrderStatus.Processed)
    {
        var contact = new Contact
        {
            Id         = Guid.NewGuid(),
            BusinessId = businessId,
            Name       = "Cliente Test",
            Phone      = "50688880000",
            CreatedAt  = DateTime.UtcNow
        };
        ctx.Contacts.Add(contact);

        var order = new RepairOrderEntity
        {
            Id             = Guid.NewGuid(),
            BusinessId     = businessId,
            OrderNumber    = "ORD-001",
            Status         = (int)status,
            IsInvoiced     = isInvoiced,
            IsActive       = !isInvoiced,
            ContactId      = contact.Id,
            Contact        = contact,
        };

        var repairItem = new RepairOrderItem
        {
            Id             = Guid.NewGuid(),
            RepairOrderId  = order.Id,
            Description    = "Cambio de pantalla",
            Price          = itemPrice,
            Quantity       = itemQty
        };
        order.Items.Add(repairItem);

        ctx.RepairOrders.Add(order);
        return order;
    }

    private static Guid GetSaleId(object result) =>
        result.GetType().GetProperty("Id")?.GetValue(result) is Guid g ? g : Guid.Empty;

    private static T GetTotalsField<T>(object result, string field)
    {
        var totals = result.GetType().GetProperty("Totals")?.GetValue(result)!;
        var value  = totals.GetType().GetProperty(field)?.GetValue(totals);
        return (T)Convert.ChangeType(value!, typeof(T));
    }

    private static List<SalePaymentMethodDto> Cash(decimal amount) =>
        [new SalePaymentMethodDto { Method = "Cash", Amount = amount }];

    // ── 1. VENTA MANUAL — cálculo correcto sin prepagos ──────────────────────

    [Fact]
    public async Task ManualSale_NoDiscount_CorrectTotals()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 10_000m);
        SeedBusiness(ctx, bizId, taxRate: 13m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            PaymentMethods = Cash(20_000m),
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 2, UnitPrice = 10_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // subtotal = 2 × 10000 = 20000 (IVA incluido)
        sale.Subtotal.Should().Be(20_000m);
        // no discount
        sale.DiscountAmount.Should().Be(0m);
        // impuesto extraído del bruto
        sale.TaxAmount.Should().Be(2_300.88m);
        // TotalOrden = bruto − descuento (sin sumar IVA)
        sale.TotalOrden.Should().Be(20_000m);
        // manual sale: PrepaidAmount = 0
        sale.PrepaidAmount.Should().Be(0m);
        // Total (cobrado hoy) = TotalOrden
        sale.Total.Should().Be(20_000m);
    }

    [Fact]
    public async Task ManualSale_LegacyDiscountColones_StillWorks()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 100_000m);
        SeedBusiness(ctx, bizId, taxRate: 13m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            Discount   = 10_000m,
            PaymentMethods = Cash(90_000m),
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 100_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.DiscountAmount.Should().Be(10_000m);
        sale.DiscountKind.Should().Be((byte)SaleDiscountKind.FixedAmount);
        sale.TotalOrden.Should().Be(90_000m);
    }

    [Fact]
    public async Task ManualSale_WithPercentDiscount_ReducesTaxableBase()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 100_000m);
        SeedBusiness(ctx, bizId, taxRate: 13m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            DiscountKind  = "Percent",
            DiscountValue = 10m,
            PaymentMethods = Cash(90_000m),
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 100_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.Subtotal.Should().Be(100_000m);
        sale.DiscountAmount.Should().Be(10_000m);
        sale.DiscountKind.Should().Be((byte)SaleDiscountKind.Percent);
        sale.DiscountInputValue.Should().Be(10m);
        sale.TaxAmount.Should().Be(10_353.98m);
        sale.TotalOrden.Should().Be(90_000m);
    }

    [Fact]
    public async Task ManualSale_WithFixedAmountDiscount_ReducesTaxableBase()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 50_000m);
        SeedBusiness(ctx, bizId, taxRate: 13m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            DiscountKind  = "FixedAmount",
            DiscountValue = 5_000m,
            PaymentMethods = Cash(45_000m),
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 50_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.DiscountAmount.Should().Be(5_000m);
        sale.DiscountKind.Should().Be((byte)SaleDiscountKind.FixedAmount);
        sale.TaxAmount.Should().Be(5_176.99m);      // extraído de 45000 bruto
        sale.TotalOrden.Should().Be(45_000m);
    }

    [Fact]
    public async Task ManualSale_ZeroTotal_AllowedWithoutPayments()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 10_000m);
        SeedBusiness(ctx, bizId, taxRate: 0m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            DiscountKind  = "FixedAmount",
            DiscountValue = 10_000m,
            PaymentMethods = [],
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 10_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.Total.Should().Be(0m);
        sale.DiscountAmount.Should().Be(10_000m);
    }

    // ── 2. VENTA DESDE REPARACIÓN — sin prepagos ─────────────────────────────

    [Fact]
    public async Task RepairSale_NoPrepaymentsNoDiscount_CorrectTotals()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 13m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 85_000m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = Cash(85_000m),
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.Subtotal.Should().Be(85_000m);
        sale.DiscountAmount.Should().Be(0m);
        sale.TaxAmount.Should().Be(9_778.76m);
        sale.TotalOrden.Should().Be(85_000m);
        sale.PrepaidAmount.Should().Be(0m);
        sale.Total.Should().Be(85_000m);
    }

    [Fact]
    public async Task RepairSale_WithPercentDiscount_AppliesAtInvoice()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 13m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 100_000m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            DiscountKind  = "Percent",
            DiscountValue = 10m,
            PaymentMethods = Cash(90_000m),
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.DiscountAmount.Should().Be(10_000m);
        sale.TaxAmount.Should().Be(10_353.98m);
        sale.TotalOrden.Should().Be(90_000m);
        sale.Total.Should().Be(90_000m);
    }

    // ── 3. VENTA DESDE REPARACIÓN — con prepagos parciales ───────────────────

    [Fact]
    public async Task RepairSale_WithPartialPrepayment_CorrectSaldoPendiente()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 13m);
        var order  = SeedRepairOrder(ctx, bizId, itemPrice: 85_000m);
        await ctx.SaveChangesAsync();

        // El cliente abonó ₡30,000 antes de facturar
        var prepayment = new Payment
        {
            Id             = Guid.NewGuid(),
            BusinessId     = bizId,
            RepairOrderId  = order.Id,
            Amount         = 30_000m,
            Method         = PaymentMethod.Cash,
            CreatedAt      = DateTime.UtcNow
        };

        var result = await CreateSut(ctx, [prepayment]).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = Cash(55_000m),
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // TotalOrden = 85000 (IVA incluido)
        sale.TotalOrden.Should().Be(85_000m);
        // PrepaidAmount = abono previo (NO es descuento)
        sale.PrepaidAmount.Should().Be(30_000m);
        // Total cobrado hoy = saldoPendiente
        sale.Total.Should().Be(55_000m);         // 85000 − 30000
        // Descuento = 0, NO suma los abonos
        sale.DiscountAmount.Should().Be(0m);
    }

    [Fact]
    public async Task RepairSale_WithMultiplePrepayments_SumsCorrectly()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 13m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 100_000m);
        await ctx.SaveChangesAsync();

        var payments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id, Amount = 20_000m, Method = PaymentMethod.Cash,     CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id, Amount = 15_000m, Method = PaymentMethod.Sinpe,    CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id, Amount = 10_000m, Method = PaymentMethod.Transfer, CreatedAt = DateTime.UtcNow }
        };

        var result = await CreateSut(ctx, payments).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = Cash(55_000m),
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // TotalOrden = 100000 (IVA incluido)
        sale.TotalOrden.Should().Be(100_000m);
        // PrepaidAmount = 20000 + 15000 + 10000 = 45000
        sale.PrepaidAmount.Should().Be(45_000m);
        // Saldo cobrado hoy
        sale.Total.Should().Be(55_000m);         // 100000 − 45000
    }

    // ── 4. TotalProfit usa TotalOrden, NO el saldo parcial ───────────────────

    [Fact]
    public async Task RepairSale_TotalProfit_BasedOnTotalOrden_NotBalance()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 500m);
        SeedBusiness(ctx, bizId, taxRate: 13m);

        // Orden: 1 ítem de 100000, costo unitario 30000
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 100_000m);
        // Agregar relación variant para calcular costo (si el item tiene CatalogVariantId)
        await ctx.SaveChangesAsync();

        // Abono de 80000 → saldo hoy = 33000, pero ganancia debe ser 113000 − costo (no 33000 − costo)
        var prepayments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id, Amount = 80_000m, Method = PaymentMethod.Cash, CreatedAt = DateTime.UtcNow }
        };

        var result = await CreateSut(ctx, prepayments).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = Cash(20_000m),
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // TotalProfit = (TotalOrden − TaxAmount) − TotalCost
        // TotalOrden = 100000, saldo hoy = 20000
        sale.TotalOrden.Should().Be(100_000m);
        sale.Total.Should().Be(20_000m);
        sale.TotalProfit.Should().Be(100_000m - sale.TaxAmount - sale.TotalCost);
        sale.TotalProfit.Should().BeGreaterThan(sale.Total);
    }

    // ── 5. SalePaymentMethods guardados correctamente ────────────────────────

    [Fact]
    public async Task ManualSale_PaymentMethods_SavedWithCorrectAmounts()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 10_000m);
        SeedBusiness(ctx, bizId);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 10_000m, ItemType = "Product" } },
            PaymentMethods =
            [
                new SalePaymentMethodDto { Method = "Cash",     Amount = 5_000m },
                new SalePaymentMethodDto { Method = "Sinpe",    Amount = 3_000m },
                new SalePaymentMethodDto { Method = "Transfer", Amount = 3_300m }
            ]
        });

        var saleId = GetSaleId(result);
        var methods = await ctx.SalePaymentMethods
            .Where(pm => pm.SaleId == saleId)
            .ToListAsync();

        methods.Should().HaveCount(3);
        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Cash     && pm.Amount == 5_000m);
        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Sinpe    && pm.Amount == 3_000m);
        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Transfer && pm.Amount == 3_300m);
    }

    [Fact]
    public async Task ManualSale_PaymentMethods_Spanish_ParsedCorrectly()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId);
        SeedBusiness(ctx, bizId);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 1m, ItemType = "Product" } },
            PaymentMethods =
            [
                new SalePaymentMethodDto { Method = "efectivo",      Amount = 1m },
                new SalePaymentMethodDto { Method = "tarjeta",       Amount = 2m },
                new SalePaymentMethodDto { Method = "transferencia", Amount = 3m },
                new SalePaymentMethodDto { Method = "sinpe",         Amount = 4m }
            ]
        });

        var saleId  = GetSaleId(result);
        var methods = await ctx.SalePaymentMethods.Where(pm => pm.SaleId == saleId).ToListAsync();

        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Cash);
        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Card);
        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Transfer);
        methods.Should().ContainSingle(pm => pm.Method == PaymentMethod.Sinpe);
    }

    [Fact]
    public async Task ManualSale_UnknownPaymentMethod_ThrowsArgumentException()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId);
        SeedBusiness(ctx, bizId);
        await ctx.SaveChangesAsync();

        var act = () => CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 1m, ItemType = "Product" } },
            PaymentMethods = [ new SalePaymentMethodDto { Method = "bitcoin", Amount = 1m } ]
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*bitcoin*");
    }

    [Fact]
    public async Task ManualSale_NegativePaymentMethodAmount_ThrowsArgumentException()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId);
        SeedBusiness(ctx, bizId);
        await ctx.SaveChangesAsync();

        var act = () => CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 1m, ItemType = "Product" } },
            PaymentMethods = [ new SalePaymentMethodDto { Method = "Cash", Amount = -500m } ]
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public async Task ManualSale_PositiveTotalWithoutPayment_ThrowsArgumentException()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId);
        SeedBusiness(ctx, bizId);
        await ctx.SaveChangesAsync();

        var act = () => CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId     = bizId,
            PaymentMethods = null!,
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 1m, ItemType = "Product" } }
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*método de pago*");
    }

    // ── 6. VALIDACIONES DE ESTADO DE REPARACIÓN ──────────────────────────────

    [Fact]
    public async Task RepairSale_AlreadyInvoiced_ThrowsInvalidOperation()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId);
        var order = SeedRepairOrder(ctx, bizId, isInvoiced: true);
        await ctx.SaveChangesAsync();

        var act = () => CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already invoiced*");
    }

    [Fact]
    public async Task RepairSale_CancelledOrder_ThrowsInvalidOperation()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId);
        var order = SeedRepairOrder(ctx, bizId, status: RepairOrderStatus.Cancelled);
        await ctx.SaveChangesAsync();

        var act = () => CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }

    [Fact]
    public async Task RepairSale_FullyPrepaid_ZeroSaldo_AllowedWithoutPayments()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 0m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 50_000m);
        await ctx.SaveChangesAsync();

        var fullPayment = new Payment
        {
            Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id,
            Amount = 50_000m, Method = PaymentMethod.Cash, CreatedAt = DateTime.UtcNow
        };

        var result = await CreateSut(ctx, [fullPayment]).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = []
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.Total.Should().Be(0m);
        sale.PrepaidAmount.Should().Be(50_000m);
        sale.TotalOrden.Should().Be(50_000m);
    }

    [Fact]
    public async Task RepairSale_PrepaymentsExceedTotal_ThrowsInvalidOperation()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 0m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 50_000m);
        await ctx.SaveChangesAsync();

        // Overpay: abonos > totalOrden
        var overpayment = new Payment
        {
            Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id,
            Amount = 60_000m, Method = PaymentMethod.Cash, CreatedAt = DateTime.UtcNow
        };

        var act = () => CreateSut(ctx, [overpayment]).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*abonos*");
    }

    // ── 7. FACTURAR DESDE REPARACIÓN marca IsInvoiced y estado Delivered ─────

    [Fact]
    public async Task RepairSale_Success_MarksOrderAsInvoicedAndDelivered()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 10_000m);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = Cash(10_000m),
        });

        await ctx.Entry(order).ReloadAsync();
        order.IsInvoiced.Should().BeTrue();
        order.InvoicedAt.Should().NotBeNull();
        order.SaleId.Should().NotBeNull();
        order.Status.Should().Be((int)RepairOrderStatus.Delivered);
    }

    [Fact]
    public async Task RepairSale_DuplicateInvoice_ThrowsInvalidOperation()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 10_000m);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var req = new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = Cash(10_000m),
        };

        // Primera factura OK
        await sut.ExecuteAsync(req);

        // Segunda factura debe fallar (IsInvoiced = true)
        var act = () => sut.ExecuteAsync(req);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── 8. RESPUESTA API incluye campos financieros nuevos ───────────────────

    [Fact]
    public async Task RepairSale_ApiResponse_ContainsTotalOrdenAndPrepaidAmount()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 13m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 85_000m);
        await ctx.SaveChangesAsync();

        var prepayment = new Payment
        {
            Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id,
            Amount = 40_000m, Method = PaymentMethod.Cash, CreatedAt = DateTime.UtcNow
        };

        var result = await CreateSut(ctx, [prepayment]).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair",
            PaymentMethods = [ new SalePaymentMethodDto { Method = "Card", Amount = 45_000m } ]
        });

        var totalOrden    = GetTotalsField<decimal>(result, "TotalOrden");
        var prepaidAmount = GetTotalsField<decimal>(result, "PrepaidAmount");
        var total         = GetTotalsField<decimal>(result, "Total");
        var discount      = GetTotalsField<decimal>(result, "Discount");

        totalOrden.Should().Be(85_000m);
        prepaidAmount.Should().Be(40_000m);
        total.Should().Be(45_000m);
        discount.Should().Be(0m);
    }

    // ── 9. VENTA MANUAL — PrepaidAmount siempre 0 ────────────────────────────

    [Fact]
    public async Task ManualSale_PrepaidAmountIsAlwaysZero()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId, price: 50_000m);
        SeedBusiness(ctx, bizId, taxRate: 13m);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            Discount   = 5_000m,
            PaymentMethods = Cash(45_000m),
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 50_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.PrepaidAmount.Should().Be(0m);
        sale.TotalOrden.Should().Be(sale.Total);  // manual: TotalOrden = Total siempre
    }

    [Fact]
    public async Task RepairSale_UpdatesContactEmailFromRequest()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 13m);
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 47_250m);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = bizId,
            RepairOrderId = order.Id,
            Discount = 5_000m,
            CustomerEmail = "cliente@example.com",
            PaymentMethods = Cash(42_250m),
            Items =
            {
                new SaleItemRequestDto
                {
                    ItemType = "Service",
                    Description = "Reparación",
                    Quantity = 1,
                    UnitPrice = 47_250m
                }
            }
        });

        var contact = await ctx.Contacts.AsNoTracking().FirstAsync(c => c.Id == order.ContactId);
        contact.Email.Should().Be("cliente@example.com");
    }
}
