using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class SoftDeleteContactUseCase : ISoftDeleteContactUseCase
{
    private readonly IAppDbContext _context;

    public SoftDeleteContactUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContactResponseDto> Execute(Guid businessId, Guid id)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);

        if (contact == null)
            throw new NotFoundException("Contact", "Contact not found.");

        if (!contact.IsDeleted)
        {
            contact.IsDeleted = true;
            contact.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        return ContactResponseMapper.ToDto(contact);
    }
}
