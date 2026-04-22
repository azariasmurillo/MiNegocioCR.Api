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
            OrderNumber = 1,
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

        var result = await sut.Execute(order.Id, request);

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
            OrderNumber = 1,
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

        await sut.Execute(order.Id, request);

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

        var act = () => sut.Execute(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "RepairOrder");
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
            OrderNumber = 1,
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

        var act = () => sut.Execute(order.Id, request);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }
}
