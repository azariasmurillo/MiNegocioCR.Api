using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken entity, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _context.PasswordResetTokens.FindAsync(new object[] { id }, cancellationToken);
        if (row == null)
            return;

        _context.PasswordResetTokens.Remove(row);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsActiveValidTokenAsync(string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.PasswordResetTokens
            .AsNoTracking()
            .AnyAsync(
                x => x.Token == tokenHash && !x.IsUsed && x.ExpiresAt > now,
                cancellationToken);
    }

    public async Task<bool> TryCompletePasswordResetAsync(string tokenHash, string newPasswordHash,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var row = await _context.PasswordResetTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.Token == tokenHash && !x.IsUsed && x.ExpiresAt > now,
                    cancellationToken);

            if (row?.User == null)
            {
                await tx.RollbackAsync(cancellationToken);
                return false;
            }

            if (!row.User.IsActive)
            {
                await tx.RollbackAsync(cancellationToken);
                return false;
            }

            row.User.PasswordHash = newPasswordHash;
            row.User.UpdatedAt = DateTime.UtcNow;
            row.IsUsed = true;

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
