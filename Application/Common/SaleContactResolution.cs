using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Resuelve o crea <see cref="ContactEntity"/> para ventas según teléfono (clave), nombre y email.
/// </summary>
public static class SaleContactResolution
{
    public static bool HasAnyContactInput(string? customerPhone, string? customerName, string? customerEmail)
    {
        if (!string.IsNullOrWhiteSpace(PhoneSanitizer.Sanitize(customerPhone)))
            return true;
        if (!string.IsNullOrWhiteSpace(customerName))
            return true;
        if (!string.IsNullOrWhiteSpace(customerEmail))
            return true;
        return false;
    }

    /// <summary>
    /// Retorna <c>null</c> si no hay datos de contacto. Con teléfono: busca/actualiza o crea. Sin teléfono pero con nombre o email: crea contacto con teléfono sintético.
    /// </summary>
    public static async Task<ContactEntity?> TryResolveOrCreateAsync(
        IAppDbContext context,
        Guid businessId,
        string? customerPhone,
        string? customerName,
        string? customerEmail,
        CancellationToken cancellationToken = default)
    {
        if (!HasAnyContactInput(customerPhone, customerName, customerEmail))
            return null;

        var sanitizedPhone = PhoneSanitizer.Sanitize(customerPhone);
        if (!string.IsNullOrWhiteSpace(sanitizedPhone))
        {
            var existing = await context.Contacts
                .AsTracking()
                .FirstOrDefaultAsync(
                    c => c.BusinessId == businessId && c.Phone == sanitizedPhone,
                    cancellationToken);

            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(customerName))
                    existing.Name = customerName.Trim();
                if (customerEmail != null)
                    existing.Email = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim();
                return existing;
            }

            var nameForNew = !string.IsNullOrWhiteSpace(customerName)
                ? customerName!.Trim()
                : "Cliente";
            var created = new ContactEntity
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                Name = nameForNew,
                Phone = sanitizedPhone,
                Email = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail!.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            context.Contacts.Add(created);
            return created;
        }

        // Sin teléfono: solo nombre y/o email → contacto con teléfono único sintético
        var syntheticPhone = "SALE-ANON-" + Guid.NewGuid().ToString("N");
        var displayName = !string.IsNullOrWhiteSpace(customerName)
            ? customerName!.Trim()
            : "Cliente";
        var anonymous = new ContactEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = displayName,
            Phone = syntheticPhone,
            Email = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail!.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        context.Contacts.Add(anonymous);
        return anonymous;
    }
}
