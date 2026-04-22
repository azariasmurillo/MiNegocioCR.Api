using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Exceptions;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;

namespace MiNegocioCR.Api.Application.Common;

public static class RepairOrderContactHelper
{
    /// <summary>
    /// Crea: usa <paramref name="contactId"/> si viene informado; si no, busca por teléfono o crea.
    /// </summary>
    public static async Task<ContactEntity> ResolveContactForCreateAsync(
        IAppDbContext context,
        Guid businessId,
        Guid? contactId,
        string customerName,
        string? customerPhone,
        string? customerEmail,
        CancellationToken cancellationToken = default)
    {
        if (contactId.HasValue)
        {
            var byId = await context.Contacts
                .FirstOrDefaultAsync(
                    c => c.Id == contactId.Value && c.BusinessId == businessId,
                    cancellationToken);
            if (byId == null)
                throw new NotFoundException("Contact", "El contacto no existe o no pertenece a este negocio.");
            return byId;
        }

        return await GetOrCreateContactAsync(
            context,
            businessId,
            customerName,
            customerPhone,
            customerEmail,
            cancellationToken);
    }

    /// <summary>
    /// Obtiene o crea un contacto por negocio + teléfono (sanitizado) y actualiza nombre/email.
    /// </summary>
    public static async Task<ContactEntity> GetOrCreateContactAsync(
        IAppDbContext context,
        Guid businessId,
        string customerName,
        string? customerPhone,
        string? customerEmail,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("El nombre del cliente es obligatorio.", nameof(customerName));

        var phone = PhoneSanitizer.Sanitize(customerPhone);
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Se requiere el teléfono del cliente.", nameof(customerPhone));

        var existing = await context.Contacts
            .FirstOrDefaultAsync(
                c => c.BusinessId == businessId && c.Phone == phone,
                cancellationToken);

        if (existing != null)
        {
            existing.Name = customerName.Trim();
            if (!string.IsNullOrWhiteSpace(customerEmail))
                existing.Email = customerEmail.Trim();
            return existing;
        }

        var contact = new ContactEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = customerName.Trim(),
            Phone = phone,
            Email = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        context.Contacts.Add(contact);
        return contact;
    }
}
