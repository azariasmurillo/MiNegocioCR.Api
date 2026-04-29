using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;

namespace MiNegocioCR.Api.Application.Contact
{
    public class Contacts : IContact
    {
        private readonly IAppDbContext _context;

        public Contacts(IAppDbContext context)
        {
            _context = context;
        }

        public async Task ImportContactsAsync(Guid businessId, List<ContactDto> contacts)
        {
            foreach (var c in contacts)
            {
                var existing = await _context.Contacts
                    .FirstOrDefaultAsync(x =>
                        x.BusinessId == businessId &&
                        x.Phone == c.Phone);

                if (existing == null)
                {
                    _context.Contacts.Add(new ContactEntity
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = businessId,
                        Name = c.Name,
                        Phone = c.Phone,
                        Email = c.Email,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existing.Name = c.Name;
                    existing.Email = c.Email;
                }
            }
            CancellationToken cancellationToken = default;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ContactDto>> GetContactsAsync(Guid businessId)
        {
            return await _context.Contacts
                .Where(x => x.BusinessId == businessId && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new ContactDto
                {
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email
                })
                .ToListAsync();
        }

    }
}
