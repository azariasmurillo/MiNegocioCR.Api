using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Domain.Entities;
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
        var businessId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Old",
            Phone = "111",
            Email = "old@test.com",
            CreatedAt = DateTime.UtcNow
        };
        context.Contacts.Add(contact);
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Pending,
            ContactId = contact.Id
        };
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var getById = new GetRepairOrderByIdUseCase(context);
        var sut = new UpdateRepairOrderUseCase(context, getById);
        var request = new UpdateRepairOrderRequestDto
        {
            Name = "New Name",
            Phone = "999",
            Email = "new@test.com",
            DeviceDescription = "Device",
            ProblemDescription = "Problem"
        };

        await sut.Execute(order.Id, request);

        var updated = await context.RepairOrders
            .AsNoTracking()
            .Include(o => o.Contact)
            .FirstAsync(o => o.Id == order.Id);
        updated.Contact.Name.Should().Be("New Name");
        updated.Contact.Phone.Should().Be("999");
        updated.Contact.Email.Should().Be("new@test.com");
        updated.DeviceDescription.Should().Be("Device");
        updated.ProblemDescription.Should().Be("Problem");
    }

    [Fact]
    public async Task Execute_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new UpdateRepairOrderUseCase(context, new GetRepairOrderByIdUseCase(context));

        var act = () => sut.Execute(Guid.NewGuid(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task Execute_WhenOrderNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new UpdateRepairOrderUseCase(context, new GetRepairOrderByIdUseCase(context));
        var request = new UpdateRepairOrderRequestDto();

        var act = () => sut.Execute(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "RepairOrder");
    }

    [Fact]
    public async Task Execute_WhenOrderIsDelivered_ThrowsArgumentException()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "X",
            Phone = "1",
            CreatedAt = DateTime.UtcNow
        };
        context.Contacts.Add(contact);
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Delivered,
            ContactId = contact.Id
        };
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var sut = new UpdateRepairOrderUseCase(context, new GetRepairOrderByIdUseCase(context));
        var request = new UpdateRepairOrderRequestDto { Name = "X" };

        var act = () => sut.Execute(order.Id, request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Delivered orders cannot be modified*");
    }

    [Fact]
    public async Task Execute_WhenOrderIsCancelled_ThrowsArgumentException()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "X",
            Phone = "1",
            CreatedAt = DateTime.UtcNow
        };
        context.Contacts.Add(contact);
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "000001",
            Status = (int)RepairOrderStatus.Cancelled,
            IsActive = false,
            ContactId = contact.Id
        };
        context.RepairOrders.Add(order);
        await context.SaveChangesAsync();

        var sut = new UpdateRepairOrderUseCase(context, new GetRepairOrderByIdUseCase(context));
        var request = new UpdateRepairOrderRequestDto { Name = "Y" };

        var act = () => sut.Execute(order.Id, request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Cancelled orders cannot be modified*");
    }
}
