using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class ListContactsUseCase : IListContactsUseCase
{
    private readonly IAppDbContext _context;

    public ListContactsUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContactResponseDto>> Execute(Guid businessId)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new ContactResponseDto
            {
                Id = c.Id,
                BusinessId = c.BusinessId,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                CreatedAt = c.CreatedAt,
                IsDeleted = c.IsDeleted,
                DeletedAt = c.DeletedAt
            })
            .ToListAsync();
    }
}
