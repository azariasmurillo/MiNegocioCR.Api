using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByFirebaseUidAsync(string firebaseUid)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid);
        }

        public async Task<User> CreateFromFirebaseAsync(string firebaseUid)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = firebaseUid
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
