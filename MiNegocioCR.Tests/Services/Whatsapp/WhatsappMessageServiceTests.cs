using System.Text.Json;
using System.Threading;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using MiNegocioCR.Api.Infrastructure.Services;
using Moq;
using Xunit;

namespace MiNegocioCR.Tests.Services.Whatsapp;

public class WhatsappMessageServiceTests
{
    private readonly Mock<IWhatsappMessageRepository> _messageRepositoryMock;
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly IAppDbContext _context;
    private readonly Mock<IAIService> _aiServiceMock;
    private readonly Mock<IWhatsappApplicationService> _whatsappAppServiceMock;
    private readonly WhatsappMessageService _sut;

    public WhatsappMessageServiceTests()
    {
        _messageRepositoryMock = new Mock<IWhatsappMessageRepository>();
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _aiServiceMock = new Mock<IAIService>();
        _whatsappAppServiceMock = new Mock<IWhatsappApplicationService>();
        _aiServiceMock.Setup(x => x.AskAsync(It.IsAny<AIRequest>())).ReturnsAsync("OK");
        _whatsappAppServiceMock
            .Setup(x => x.SendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), null, null))
            .Returns(Task.CompletedTask);
        _messageRepositoryMock.Setup(x => x.MessageExistsByMetaIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _messageRepositoryMock.Setup(x => x.UpdateConversationAfterMessageAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<MessageDirection>()))
            .Returns(Task.CompletedTask);
        _sut = new WhatsappMessageService(
            _messageRepositoryMock.Object,
            _businessRepositoryMock.Object,
            _context,
            _aiServiceMock.Object,
            _whatsappAppServiceMock.Object,
            NullLogger<WhatsappMessageService>.Instance);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithEmptyEntry_ReturnsWithoutSaving()
    {
        var payload = JsonDocument.Parse("{}").RootElement;

        await _sut.ProcessWebhookAsync(payload);

        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<WhatsAppMessage>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithValidIncomingMessage_SavesMessage()
    {
        var business = new Business { Id = Guid.NewGuid() };
        _context.Businesses.Add(business);
        _context.BusinessSettings.Add(new BusinessSettings
        {
            BusinessId = business.Id,
            EnableAIChat = false,
            NextOrderNumber = 1
        });
        await _context.SaveChangesAsync(CancellationToken.None);

        var convId = Guid.NewGuid();
        var conv = new WhatsAppConversation
        {
            Id = convId,
            BusinessId = business.Id,
            PhoneNumber = "1234567890"
        };
        _businessRepositoryMock
            .Setup(x => x.GetByWhatsappPhoneNumberIdAsync("phone-id"))
            .ReturnsAsync(business);
        _messageRepositoryMock.Setup(x => x.GetOrCreateConversationAsync(business.Id, "1234567890", null))
            .ReturnsAsync(conv);
        _messageRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<WhatsAppMessage>())).Returns(Task.CompletedTask);

        var json = """
            {
                "entry": [{
                    "changes": [{
                        "value": {
                            "messages": [{
                                "id": "msg-1",
                                "from": "1234567890",
                                "text": { "body": "Hello" }
                            }],
                            "metadata": { "phone_number_id": "phone-id" }
                        }
                    }]
                }]
            }
            """;
        var payload = JsonDocument.Parse(json).RootElement;

        await _sut.ProcessWebhookAsync(payload);

        _messageRepositoryMock.Verify(
            x => x.SaveAsync(It.Is<WhatsAppMessage>(m =>
                m.MessageId == "msg-1" &&
                m.From == "1234567890" &&
                m.Body == "Hello" &&
                m.ConversationId == convId &&
                m.Direction == MessageDirection.Inbound &&
                m.Status == MessageStatus.Received)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WhenBusinessNotFoundForPhone_DoesNotSaveMessage()
    {
        _businessRepositoryMock
            .Setup(x => x.GetByWhatsappPhoneNumberIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Business?)null);

        var json = """
            {
                "entry": [{
                    "changes": [{
                        "value": {
                            "messages": [{
                                "id": "msg-1",
                                "from": "1234567890",
                                "text": { "body": "Hi" }
                            }],
                            "metadata": { "phone_number_id": "unknown-phone" }
                        }
                    }]
                }]
            }
            """;
        var payload = JsonDocument.Parse(json).RootElement;

        await _sut.ProcessWebhookAsync(payload);

        _messageRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<WhatsAppMessage>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithStatusUpdate_CallsUpdateStatusAsync()
    {
        _messageRepositoryMock.Setup(x => x.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<MessageStatus>()))
            .Returns(Task.CompletedTask);

        var json = """
            {
                "entry": [{
                    "changes": [{
                        "value": {
                            "statuses": [{
                                "id": "msg-1",
                                "status": "delivered"
                            }]
                        }
                    }]
                }]
            }
            """;
        var payload = JsonDocument.Parse(json).RootElement;

        await _sut.ProcessWebhookAsync(payload);

        _messageRepositoryMock.Verify(
            x => x.UpdateStatusAsync("msg-1", MessageStatus.Delivered),
            Times.Once);
    }
}
