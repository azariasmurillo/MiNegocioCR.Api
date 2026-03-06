using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.UseCases.Business;
using MiNegocioCR.Api.Domain.Exceptions;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Xunit;

namespace MiNegocioCR.Tests.UseCases.Business;

public class SetBusinessActiveStatusUseCaseTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Execute_WithExistingBusiness_UpdatesIsActive()
    {
        await using var context = CreateInMemoryContext();
        var business = new BusinessEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            IsActive = true
        };
        context.Businesses.Add(business);
        await context.SaveChangesAsync();

        var sut = new SetBusinessActiveStatusUseCase(context);

        await sut.Execute(business.Id, isActive: false);

        await context.Entry(business).ReloadAsync();
        business.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenBusinessNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateInMemoryContext();
        var sut = new SetBusinessActiveStatusUseCase(context);
        var nonExistentId = Guid.NewGuid();

        var act = () => sut.Execute(nonExistentId, true);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Resource == "Business" && ex.Message.Contains("not found"));
    }
}
