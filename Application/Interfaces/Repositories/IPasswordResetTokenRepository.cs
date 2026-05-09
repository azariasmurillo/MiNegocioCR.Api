using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Token ya persistido como hash (hex SHA-256).</summary>
    Task<bool> IsActiveValidTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza contraseña del usuario y marca el token como usado; todo en una transacción.
    /// </summary>
    Task<bool> TryCompletePasswordResetAsync(string tokenHash, string newPasswordHash,
        CancellationToken cancellationToken = default);
}
