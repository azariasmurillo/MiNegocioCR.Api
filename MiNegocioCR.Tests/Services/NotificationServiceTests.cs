using FluentAssertions;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Services;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _sut = new NotificationService(_emailServiceMock.Object);
    }

    [Fact]
    public async Task OrderCreatedAsync_WhenOrderAndBusinessValidAndEmailEnabled_SendsEmail()
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            EnableEmailNotifications = true
        };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "Cliente",
            Phone = "50688888888",
            Email = "client@test.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "000042",
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };
        _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.OrderCreatedAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                business,
                "client@test.com",
                $"Orden #{order.OrderNumber} creada",
                It.Is<string>(b => b.Contains("Orden creada") && b.Contains(order.OrderNumber))),
            Times.Once);
    }

    [Fact]
    public async Task OrderCreatedAsync_WhenEmailNotificationsDisabled_DoesNotCallEmailService()
    {
        var business = new Business { EnableEmailNotifications = false };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "a@b.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "000001",
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };

        await _sut.OrderCreatedAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task OrderCreatedAsync_WhenCustomerEmailIsEmpty_DoesNotCallEmailService()
    {
        var business = new Business { EnableEmailNotifications = true };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "000001",
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };

        await _sut.OrderCreatedAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task OrderProcessedAsync_WhenEmailEnabled_SendsEmail()
    {
        var business = new Business { EnableEmailNotifications = true };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "a@b.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };
        _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.OrderProcessedAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(business, "a@b.com", It.Is<string>(s => s.Contains("proceso")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task OrderProcessedAsync_WhenEmailDisabled_DoesNotCallEmailService()
    {
        var business = new Business { EnableEmailNotifications = false };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "a@b.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };

        await _sut.OrderProcessedAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task OrderDeliveredAsync_WhenEmailEnabled_SendsEmail()
    {
        var business = new Business { EnableEmailNotifications = true };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "a@b.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };
        _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.OrderDeliveredAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(business, "a@b.com", It.Is<string>(s => s.Contains("entrega")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task OrderCancelledAsync_WhenEmailEnabled_SendsEmail()
    {
        var business = new Business { EnableEmailNotifications = true };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "a@b.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };
        _emailServiceMock.Setup(x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.OrderCancelledAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(business, "a@b.com", It.Is<string>(s => s.Contains("cancelada")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task OrderCancelledAsync_WhenEmailDisabled_DoesNotCallEmailService()
    {
        var business = new Business { EnableEmailNotifications = false };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "A",
            Phone = "1",
            Email = "a@b.com",
            CreatedAt = DateTime.UtcNow
        };
        var order = new RepairOrder
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            ContactId = contact.Id,
            Contact = contact,
            Business = business
        };

        await _sut.OrderCancelledAsync(business, order);

        _emailServiceMock.Verify(
            x => x.SendAsync(It.IsAny<Business>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
