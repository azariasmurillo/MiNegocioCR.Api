using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.UseCases.Contacts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Contacts;

public class UpdateContactUseCaseTests
{
    private static AppDbContext CreateInMemoryContext(QueryTrackingBehavior tracking = QueryTrackingBehavior.TrackAll)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseQueryTrackingBehavior(tracking)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WithGlobalNoTracking_PersistsNameChange()
    {
        await using var context = CreateInMemoryContext(QueryTrackingBehavior.NoTracking);
        var businessId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        context.Contacts.Add(new Contact
        {
            Id = contactId,
            BusinessId = businessId,
            Name = "Nombre mal escrito",
            Phone = "88887777",
            Email = "viejo@test.com",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var sut = new UpdateContactUseCase(context);
        var result = await sut.Execute(businessId, contactId, new UpdateContactRequestDto
        {
            Name = "Nombre corregido",
            Phone = "88887777",
            Email = "nuevo@test.com"
        });

        result.Name.Should().Be("Nombre corregido");
        result.Email.Should().Be("nuevo@test.com");

        context.ChangeTracker.Clear();
        var stored = await context.Contacts.AsNoTracking().FirstAsync(c => c.Id == contactId);
        stored.Name.Should().Be("Nombre corregido");
        stored.Email.Should().Be("nuevo@test.com");
    }
}
