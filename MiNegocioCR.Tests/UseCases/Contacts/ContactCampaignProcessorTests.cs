using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Contacts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Moq;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Tests.UseCases.Contacts;

public class ContactCampaignProcessorTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    [Fact]
    public async Task ProcessNextAsync_DoesNotResendAfterRecipientMarkedSent()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        ctx.Businesses.Add(new BusinessEntity
        {
            Id = businessId,
            Name = "JoyCaTech",
            PublicEmail = "joyca@test.com",
            EnableEmailNotifications = true,
            CreatedAt = DateTime.UtcNow
        });
        ctx.Contacts.Add(new Contact
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Alexander",
            Phone = "50688880001",
            Email = "alex@test.com",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow.AddDays(-100)
        });
        ctx.EmailCampaigns.Add(new EmailCampaign
        {
            Id = campaignId,
            BusinessId = businessId,
            SubjectTemplate = "Promo navideña",
            BodyText = "Hola {nombre}, tenemos promoción especial hasta fin de mes.",
            Status = "Queued",
            CreatedAt = DateTime.UtcNow,
            TotalRecipients = 1
        });
        ctx.EmailCampaignRecipients.Add(new EmailCampaignRecipient
        {
            Id = recipientId,
            CampaignId = campaignId,
            ContactId = contactId,
            ContactName = "Alexander",
            ContactEmail = "alex@test.com",
            Status = CampaignQueueRecipientStatus.Pending,
            GlobalQueueOrder = 1
        });
        await ctx.SaveChangesAsync();

        var email = new Mock<IEmailService>();
        email.Setup(x => x.SendCampaignAsync(
                It.IsAny<BusinessEntity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("msg-1");

        var sut = new CampaignQueueProcessor(ctx, email.Object, NullLogger<CampaignQueueProcessor>.Instance);

        (await sut.ProcessNextAsync()).Should().BeTrue();
        (await sut.ProcessNextAsync()).Should().BeFalse();

        email.Verify(
            x => x.SendCampaignAsync(
                It.IsAny<BusinessEntity>(),
                "alex@test.com",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var recipient = await ctx.EmailCampaignRecipients.SingleAsync();
        recipient.Status.Should().Be(CampaignQueueRecipientStatus.Sent);
    }
}
