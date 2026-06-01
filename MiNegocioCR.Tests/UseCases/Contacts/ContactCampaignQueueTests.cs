using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.UseCases.Contacts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Xunit;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Tests.UseCases.Contacts;

public class ContactCampaignQueueTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    [Fact]
    public async Task QueueCampaign_CreatesRecipientsWithGlobalOrder()
    {
        await using var ctx = CreateContext();
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

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
            Name = "Ana García",
            Phone = "50688880001",
            Email = "ana@test.com",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow.AddDays(-100)
        });
        await ctx.SaveChangesAsync();

        var sut = new QueueCampaignUseCase(ctx);
        var result = await sut.Execute(businessId, new QueueCampaignRequestDto
        {
            ContactIds = [contactId],
            Subject = "Feliz Navidad",
            BodyText = "Hola {nombre}",
            InactiveDays = 60,
            QuietDays = 60
        });

        result.TotalRecipients.Should().Be(1);
        result.CampaignId.Should().NotBeEmpty();
        (await ctx.EmailCampaignRecipients.CountAsync()).Should().Be(1);
        var recipient = await ctx.EmailCampaignRecipients.SingleAsync();
        recipient.GlobalQueueOrder.Should().Be(1);
        recipient.Status.Should().Be("Pending");
    }
}
