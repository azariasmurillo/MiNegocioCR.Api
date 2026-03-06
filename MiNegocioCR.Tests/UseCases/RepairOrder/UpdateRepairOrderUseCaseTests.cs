using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Domain.Enums;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.RepairOrder;

public class UpdateRepairOrderUseCaseTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WithValidRequest_UpdatesOrderFields()
    {
        await using var context = CreateInMemoryContext();
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            OrderNumber = 1,
            Status = (int)RepairOrderStatus.Pending,
            CustomerName = "Old",
            CustomerPhone = "111",
            CustomerEmail = "old@test.com"
        };
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var sut = new UpdateRepairOrderUseCase(context);
        var request = new UpdateRepairOrderRequestDto
        {
            CustomerName = "New Name",
            CustomerPhone = "999",
            CustomerEmail = "new@test.com",
            DeviceDescription = "Device",
            ProblemDescription = "Problem"
        };

        await sut.Execute(order.Id, request);

        await context.Entry(order).ReloadAsync();
        order.CustomerName.Should().Be("New Name");
        order.CustomerPhone.Should().Be("999");
        order.CustomerEmail.Should().Be("new@test.com");
        order.DeviceDescription.Should().Be("Device");
        order.ProblemDescription.Should().Be("Problem");
    }

    [Fact]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new UpdateRepairOrderUseCase(context);

        var act = () => sut.Execute(Guid.NewGuid(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task Execute_WhenOrderNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new UpdateRepairOrderUseCase(context);
        var request = new UpdateRepairOrderRequestDto { CustomerName = "X" };

        var act = () => sut.Execute(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "RepairOrder");
    }

    [Fact]
    public async Task Execute_WhenOrderIsDelivered_ThrowsArgumentException()
    {
        await using var context = CreateInMemoryContext();
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            OrderNumber = 1,
            Status = (int)RepairOrderStatus.Delivered
        };
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var sut = new UpdateRepairOrderUseCase(context);
        var request = new UpdateRepairOrderRequestDto { CustomerName = "X" };

        var act = () => sut.Execute(order.Id, request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Delivered orders cannot be modified*");
    }
}
