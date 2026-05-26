using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.RepairOrder;

public class GetRepairOrderByIdUseCaseTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_IncludesItems_InJsonResponseShape()
    {
        await using var context = CreateInMemoryContext();
        var businessId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Cliente",
            Phone = "50688888888",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "20260526001",
            Status = (int)RepairOrderStatus.Pending,
            ContactId = contact.Id,
            Brand = "DELL",
            Model = "1255"
        };
        var item = new RepairOrderItem
        {
            Id = Guid.NewGuid(),
            RepairOrderId = order.Id,
            Description = "Memoria RAM 8GB",
            Quantity = 1,
            Price = 25000m
        };
        context.Contacts.Add(contact);
        context.RepairOrders.Add(order);
        context.RepairOrderItems.Add(item);
        await context.SaveChangesAsync();

        var sut = new GetRepairOrderByIdUseCase(context);
        var result = await sut.Execute(businessId, order.Id);

        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        json.Should().Contain("\"items\"");
        json.Should().Contain("Memoria RAM 8GB");

        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        items[0].GetProperty("description").GetString().Should().Be("Memoria RAM 8GB");
    }
}
