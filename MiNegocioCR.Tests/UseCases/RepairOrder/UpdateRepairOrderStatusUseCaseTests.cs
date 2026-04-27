using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Domain.Enums;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Domain.Entities;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.RepairOrder;

public class UpdateRepairOrderStatusUseCaseTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WithValidTransition_PendingToInProcess_UpdatesStatusAndSaves()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var business = new BusinessEntity { Id = businessId, Name = "Test" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "C",
            Phone = "50611111111",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Pending,
            ContactId = contact.Id
        };
        context.Businesses.Add(business);
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var request = new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.InProcess };

        var result = await sut.Execute(businessId, order.Id, request);

        result.Should().NotBeNull();
        await context.Entry(order).ReloadAsync();
        order.Status.Should().Be((int)RepairOrderStatus.InProcess);
    }

    [Fact]
    public async Task Execute_WhenTransitionToProcessed_InvokesOrderProcessedAsync()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var business = new BusinessEntity { Id = businessId, Name = "Test" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "C",
            Phone = "50622222222",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.InProcess,
            ContactId = contact.Id
        };
        context.Businesses.Add(business);
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var request = new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.Processed };

        await sut.Execute(businessId, order.Id, request);

        notificationMock.Verify(
            x => x.OrderProcessedAsync(It.IsAny<BusinessEntity>(), It.Is<RepairOrderEntity>(o => o.Id == order.Id)),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenOrderNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateInMemoryContext();
        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var request = new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.InProcess };

        var act = () => sut.Execute(Guid.NewGuid(), Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "RepairOrder");
    }

    [Fact]
    public async Task Execute_WhenSameAsCurrent_ReturnsStatusStringWithoutReNotifications()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var business = new BusinessEntity { Id = businessId, Name = "Test" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "C",
            Phone = "50644444444",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Pending,
            ContactId = contact.Id,
            UpdatedAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc)
        };
        context.Businesses.Add(business);
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();
        var updatedAtBefore = order.UpdatedAt;

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var result = await sut.Execute(
            businessId,
            order.Id,
            new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.Pending });
        var statusValue = result!.GetType().GetProperty("Status")?.GetValue(result) as string;
        statusValue.Should().Be(RepairOrderStatus.Pending.ToString());
        await context.Entry(order).ReloadAsync();
        order.Status.Should().Be((int)RepairOrderStatus.Pending);
        order.UpdatedAt.Should().BeCloseTo(updatedAtBefore, TimeSpan.FromSeconds(1));
        notificationMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_WhenProcessedToCancelled_ThrowsInvalidStatusTransitionException()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var business = new BusinessEntity { Id = businessId, Name = "Test" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "C",
            Phone = "50655555555",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Processed,
            ContactId = contact.Id
        };
        context.Businesses.Add(business);
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var act = () => sut.Execute(
            businessId,
            order.Id,
            new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.Cancelled });

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }

    [Fact]
    public async Task Execute_WhenInvalidTransition_ThrowsInvalidStatusTransitionException()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var business = new BusinessEntity { Id = businessId, Name = "Test" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "C",
            Phone = "50633333333",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Pending,
            ContactId = contact.Id
        };
        context.Businesses.Add(business);
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var request = new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.Delivered };

        var act = () => sut.Execute(businessId, order.Id, request);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }

    [Fact]
    public async Task Execute_Cancelled_SetsIsActiveFalse()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var business = new BusinessEntity { Id = businessId, Name = "Test" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "C",
            Phone = "50633333333",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Pending,
            IsActive = true,
            ContactId = contact.Id
        };
        context.Businesses.Add(business);
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        await sut.Execute(businessId, order.Id, new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.Cancelled });

        order.IsActive.Should().BeFalse();
        order.Status.Should().Be((int)RepairOrderStatus.Cancelled);
    }
}
