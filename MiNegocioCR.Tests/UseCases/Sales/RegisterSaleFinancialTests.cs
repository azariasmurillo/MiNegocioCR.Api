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
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 2, UnitPrice = 10_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // subtotal = 2 × 10000 = 20000
        sale.Subtotal.Should().Be(20_000m);
        // no discount
        sale.DiscountAmount.Should().Be(0m);
        // tax = 20000 × 13% = 2600
        sale.TaxAmount.Should().Be(2_600m);
        // TotalOrden = TaxableBase + Tax = 20000 + 2600 = 22600
        sale.TotalOrden.Should().Be(22_600m);
        // manual sale: PrepaidAmount = 0
        sale.PrepaidAmount.Should().Be(0m);
        // Total (cobrado hoy) = TotalOrden
        sale.Total.Should().Be(22_600m);
    }

    [Fact]
    public async Task ManualSale_WithDiscount_ReducesTaxableBase()
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
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 100_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // subtotal = 100000, discount = 10000, taxable = 90000
        sale.Subtotal.Should().Be(100_000m);
        sale.DiscountAmount.Should().Be(10_000m);
        // tax = 90000 × 13% = 11700
        sale.TaxAmount.Should().Be(11_700m);
        sale.TotalOrden.Should().Be(101_700m);
        sale.Total.Should().Be(101_700m);
        // CLAVE: prepagos NO son descuentos
        sale.PrepaidAmount.Should().Be(0m);
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
            Source        = "Repair"
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.Subtotal.Should().Be(85_000m);
        sale.DiscountAmount.Should().Be(0m);
        sale.TaxAmount.Should().Be(11_050m);     // 85000 × 13%
        sale.TotalOrden.Should().Be(96_050m);
        sale.PrepaidAmount.Should().Be(0m);
        sale.Total.Should().Be(96_050m);         // saldo cobrado hoy = totalOrden
    }

    [Fact]
    public async Task RepairSale_NoOrderDiscount_DiscountAmountIsZero()
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
            Source        = "Repair"
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.Subtotal.Should().Be(85_000m);
        sale.DiscountAmount.Should().Be(0m);
        sale.TaxAmount.Should().Be(11_050m);
        sale.TotalOrden.Should().Be(96_050m);
        sale.PrepaidAmount.Should().Be(0m);
        sale.Total.Should().Be(96_050m);
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
            Source        = "Repair"
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // TotalOrden = 85000 + 11050 (13%) = 96050
        sale.TotalOrden.Should().Be(96_050m);
        // PrepaidAmount = abono previo (NO es descuento)
        sale.PrepaidAmount.Should().Be(30_000m);
        // Total cobrado hoy = saldoPendiente
        sale.Total.Should().Be(66_050m);         // 96050 − 30000
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
            Source        = "Repair"
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // TotalOrden = 100000 + 13000 = 113000
        sale.TotalOrden.Should().Be(113_000m);
        // PrepaidAmount = 20000 + 15000 + 10000 = 45000
        sale.PrepaidAmount.Should().Be(45_000m);
        // Saldo cobrado hoy
        sale.Total.Should().Be(68_000m);         // 113000 − 45000
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
            Source        = "Repair"
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        // TotalProfit = TotalOrden − TotalCost (el costo es 0 si no hay variant costo)
        // TotalOrden = 113000, saldo hoy = 33000
        // TotalProfit debe ser ≥ saldo (no calcular sobre saldo)
        sale.TotalOrden.Should().Be(113_000m);
        sale.Total.Should().Be(33_000m);          // saldo cobrado hoy
        // TotalProfit basado en TotalOrden
        sale.TotalProfit.Should().Be(113_000m - sale.TotalCost);
        sale.TotalProfit.Should().BeGreaterThan(sale.Total);  // ganancia > saldo parcial
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
    public async Task ManualSale_NullPaymentMethods_DoesNotThrow()
    {
        await using var ctx = CreateContext();
        var bizId     = Guid.NewGuid();
        var variantId = SeedVariant(ctx, bizId);
        SeedBusiness(ctx, bizId);
        await ctx.SaveChangesAsync();

        // PaymentMethods = null (null-safe foreach)
        var result = await CreateSut(ctx).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId     = bizId,
            PaymentMethods = null!,
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 1m, ItemType = "Product" } }
        });

        GetSaleId(result).Should().NotBeEmpty();
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
    public async Task RepairSale_FullyPaidOrder_ThrowsInvalidOperation()
    {
        await using var ctx = CreateContext();
        var bizId = Guid.NewGuid();
        SeedBusiness(ctx, bizId, taxRate: 0m);  // sin IVA para simplificar
        var order = SeedRepairOrder(ctx, bizId, itemPrice: 50_000m);
        await ctx.SaveChangesAsync();

        // El cliente ya pagó el 100% de la orden
        var fullPayment = new Payment
        {
            Id = Guid.NewGuid(), BusinessId = bizId, RepairOrderId = order.Id,
            Amount = 50_000m, Method = PaymentMethod.Cash, CreatedAt = DateTime.UtcNow
        };

        var act = () => CreateSut(ctx, [fullPayment]).ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId    = bizId,
            RepairOrderId = order.Id,
            Source        = "Repair"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*pagada*");
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
            Source        = "Repair"
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
        var req = new CreateSaleRequestDto { BusinessId = bizId, RepairOrderId = order.Id, Source = "Repair" };

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
            PaymentMethods = [ new SalePaymentMethodDto { Method = "Card", Amount = 56_050m } ]
        });

        var totalOrden    = GetTotalsField<decimal>(result, "TotalOrden");
        var prepaidAmount = GetTotalsField<decimal>(result, "PrepaidAmount");
        var total         = GetTotalsField<decimal>(result, "Total");
        var discount      = GetTotalsField<decimal>(result, "Discount");

        totalOrden.Should().Be(96_050m);
        prepaidAmount.Should().Be(40_000m);
        total.Should().Be(56_050m);
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
            Items = { new SaleItemRequestDto { CatalogVariantId = variantId, Quantity = 1, UnitPrice = 50_000m, ItemType = "Product" } }
        });

        var saleId = GetSaleId(result);
        var sale   = await ctx.Sales.AsNoTracking().FirstAsync(s => s.Id == saleId);

        sale.PrepaidAmount.Should().Be(0m);
        sale.TotalOrden.Should().Be(sale.Total);  // manual: TotalOrden = Total siempre
    }
}
