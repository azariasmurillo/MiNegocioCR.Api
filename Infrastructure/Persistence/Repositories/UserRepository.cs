using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Users
            .AsNoTracking()
            .Include(x => x.Business)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalized);
    }

    public async Task<User?> GetByIdWithBusinessAsync(Guid userId)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(x => x.Business)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
