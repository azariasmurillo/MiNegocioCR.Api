using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class SearchContactsUseCase : ISearchContactsUseCase
{
    private readonly IAppDbContext _context;

    public SearchContactsUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContactResponseDto>> Execute(Guid businessId, string? query)
    {
        var q = _context.Contacts
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLower();
            q = q.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Phone.ToLower().Contains(term) ||
                (c.Email != null && c.Email.ToLower().Contains(term)));
        }

        return await q
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
