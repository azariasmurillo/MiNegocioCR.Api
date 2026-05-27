using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class RepairOrderContactHelperTests
{
    private static AppDbContext CreateContext(QueryTrackingBehavior tracking = QueryTrackingBehavior.TrackAll)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseQueryTrackingBehavior(tracking)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ResolveContactForCreateAsync_WithGlobalNoTracking_ReusesExistingContactById()
    {
        await using var ctx = CreateContext(QueryTrackingBehavior.NoTracking);
        var businessId = Guid.NewGuid();
        var existing = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Cliente",
            Phone = "62855599",
            CreatedAt = DateTime.UtcNow
        };
        ctx.Contacts.Add(existing);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        var contact = await RepairOrderContactHelper.ResolveContactForCreateAsync(
            ctx,
            businessId,
            existing.Id,
            "Otro nombre",
            "62855599",
            "cliente@example.com");

        contact.Id.Should().Be(existing.Id);

        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "20260525001",
            ContactId = contact.Id,
            Status = (int)RepairOrderStatus.Pending,
            IsActive = true
        };
        ctx.RepairOrders.Add(order);
        await ctx.SaveChangesAsync();

        (await ctx.Contacts.CountAsync()).Should().Be(1);
        var persisted = await ctx.RepairOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        persisted.ContactId.Should().Be(existing.Id);
    }

    [Fact]
    public async Task GetOrCreateContactAsync_WithGlobalNoTracking_ReusesExistingContactByPhone()
    {
        await using var ctx = CreateContext(QueryTrackingBehavior.NoTracking);
        var businessId = Guid.NewGuid();
        var existing = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Cliente",
            Phone = "62855599",
            CreatedAt = DateTime.UtcNow
        };
        ctx.Contacts.Add(existing);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        var contact = await RepairOrderContactHelper.GetOrCreateContactAsync(
            ctx,
            businessId,
            "Cliente actualizado",
            "62855599",
            "cliente@example.com");

        contact.Id.Should().Be(existing.Id);

        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = "20260525002",
            ContactId = contact.Id,
            Status = (int)RepairOrderStatus.Pending,
            IsActive = true
        };
        ctx.RepairOrders.Add(order);
        await ctx.SaveChangesAsync();

        (await ctx.Contacts.CountAsync()).Should().Be(1);
        var updated = await ctx.Contacts.AsNoTracking().FirstAsync(c => c.Id == existing.Id);
        updated.Name.Should().Be("Cliente actualizado");
        updated.Email.Should().Be("cliente@example.com");
    }
}
