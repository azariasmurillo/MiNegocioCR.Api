using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class UpdateContactUseCase : IUpdateContactUseCase
{
    private readonly IAppDbContext _context;

    public UpdateContactUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContactResponseDto> Execute(Guid businessId, Guid id, UpdateContactRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException("Phone is required.", nameof(request.Phone));
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        var normalizedPhone = request.Phone.Trim();

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id && !c.IsDeleted);

        if (contact == null)
            throw new NotFoundException("Contact", "Contact not found.");

        var duplicate = await _context.Contacts.AnyAsync(c =>
            c.BusinessId == businessId &&
            !c.IsDeleted &&
            c.Id != id &&
            c.Phone == normalizedPhone);

        if (duplicate)
            throw new ArgumentException("Another contact with this phone already exists in this business.");

        contact.Name = request.Name.Trim();
        contact.Phone = normalizedPhone;
        contact.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        await _context.SaveChangesAsync(CancellationToken.None);

        return ContactResponseMapper.ToDto(contact);
    }
}
