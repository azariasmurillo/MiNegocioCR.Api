using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories;

public interface IUserRepository
{
    /// <summary>Incluye <see cref="User.Business"/> para sesión.</summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Incluye <see cref="User.Business"/> para sesión.</summary>
    Task<User?> GetByIdWithBusinessAsync(Guid userId);
}
