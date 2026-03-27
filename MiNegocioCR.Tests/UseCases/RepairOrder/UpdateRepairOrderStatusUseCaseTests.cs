using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Domain.Enums;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
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
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            OrderNumber = 1,
            Status = (int)RepairOrderStatus.Pending
        };
        var business = new BusinessEntity { Id = order.BusinessId, Name = "Test" };
        context.Businesses.Add(business);
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
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            OrderNumber = 1,
            Status = (int)RepairOrderStatus.InProcess
        };
        var business = new BusinessEntity { Id = order.BusinessId, Name = "Test" };
        context.Businesses.Add(business);
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
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            OrderNumber = 1,
            Status = (int)RepairOrderStatus.Pending
        };
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var notificationMock = new Mock<INotificationService>();
        var sut = new UpdateRepairOrderStatusUseCase(context, notificationMock.Object);
        var request = new UpdateStatusRequestDto { NewStatus = RepairOrderStatus.Delivered };

        var act = () => sut.Execute(order.Id, request);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }
}
