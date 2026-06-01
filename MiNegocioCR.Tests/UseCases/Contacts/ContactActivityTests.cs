using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.UseCases.Contacts;
using MiNegocioCR.Api.Application.UseCases.Payments;
using MiNegocioCR.Api.Application.UseCases.Sales;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;

namespace MiNegocioCR.Tests.UseCases.Contacts;

public class ContactActivityTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private static RegisterSaleUseCase CreateSaleSut(AppDbContext ctx, IEnumerable<Payment>? repairPayments = null)
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
    public void Touch_UsesLatestTimestamp()
    {
        var contact = new ContactEntity { LastActivityAt = DateTime.UtcNow.AddDays(-10) };
        var newer = DateTime.UtcNow.AddDays(-1);

        ContactActivityHelper.Touch(contact, newer);

        contact.LastActivityAt.Should().Be(newer);
    }

    [Fact]
    public void Touch_DoesNotMoveBackward()
    {
        var latest = DateTime.UtcNow.AddDays(-1);
        var contact = new ContactEntity { LastActivityAt = latest };

        ContactActivityHelper.Touch(contact, latest.AddDays(-5));

        contact.LastActivityAt.Should().Be(latest);
    }

    [Fact]
    public async Task RegisterSale_ManualSale_UpdatesLastActivityAt()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity
        {
            Id = businessId,
            Name = "Test",
            TaxRatePercent = 13m,
            CreatedAt = DateTime.UtcNow
        });
        ctx.Contacts.Add(new ContactEntity
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Ana",
            Phone = "50688880000",
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var sut = CreateSaleSut(ctx);
        await sut.ExecuteAsync(new CreateSaleRequestDto
        {
            BusinessId = businessId,
            ContactId = contactId,
            Items =
            [
                new SaleItemRequestDto
                {
                    ItemType = "Service",
                    Description = "Servicio",
                    Quantity = 1,
                    UnitPrice = 10_000m
                }
            ],
            PaymentMethods =
            [
                new SalePaymentMethodDto { Method = "Cash", Amount = 11_300m }
            ]
        });

        var contact = await ctx.Contacts.AsNoTracking().SingleAsync(c => c.Id == contactId);
        contact.LastActivityAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRepairOrder_DoesNotUpdateLastActivityAt()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity { Id = businessId, Name = "Test", CreatedAt = DateTime.UtcNow });
        ctx.Contacts.Add(new ContactEntity
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Luis",
            Phone = "50688881111",
            CreatedAt = DateTime.UtcNow
        });
        ctx.RepairOrders.Add(new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ContactId = contactId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var contact = await ctx.Contacts.AsNoTracking().SingleAsync(c => c.Id == contactId);
        contact.LastActivityAt.Should().BeNull();
    }

    [Fact]
    public async Task CreatePayment_UpdatesLastActivityAt()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity { Id = businessId, Name = "Test", CreatedAt = DateTime.UtcNow });
        ctx.Contacts.Add(new ContactEntity
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Carlos",
            Phone = "50688882222",
            CreatedAt = DateTime.UtcNow
        });
        ctx.RepairOrders.Add(new RepairOrderEntity
        {
            Id = orderId,
            BusinessId = businessId,
            ContactId = contactId,
            OrderNumber = "000002",
            Status = (int)RepairOrderStatus.InProcess,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var sut = new CreatePaymentUseCase(ctx);
        await sut.Execute(new CreatePaymentRequestDto
        {
            BusinessId = businessId,
            RepairOrderId = orderId,
            Amount = 5_000m,
            Type = PaymentType.Advance,
            Method = PaymentMethod.Cash
        });

        var contact = await ctx.Contacts.AsNoTracking().SingleAsync(c => c.Id == contactId);
        contact.LastActivityAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ListContactInsights_MarksInactiveContacts()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var activeId = Guid.NewGuid();
        var inactiveId = Guid.NewGuid();

        ctx.Contacts.AddRange(
            new ContactEntity
            {
                Id = activeId,
                BusinessId = businessId,
                Name = "Activo",
                Phone = "50688883333",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-5)
            },
            new ContactEntity
            {
                Id = inactiveId,
                BusinessId = businessId,
                Name = "Inactivo",
                Phone = "50688884444",
                Email = "inactivo@test.com",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow.AddDays(-90)
            });
        await ctx.SaveChangesAsync();

        var sut = new ListContactInsightsUseCase(ctx);
        var result = await sut.Execute(businessId, inactiveDays: 60);

        result.Summary.TotalContacts.Should().Be(2);
        result.Summary.InactiveCount.Should().Be(1);
        result.Summary.ActiveCount.Should().Be(1);
        result.Contacts.Single(c => c.Id == inactiveId).IsInactive.Should().BeTrue();
        result.Contacts.Single(c => c.Id == activeId).IsInactive.Should().BeFalse();
    }
}
