using MiNegocioCR.Api.Domain.Entities;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;

namespace MiNegocioCR.Api.Application.Common;

public static class CampaignRecipientDeduplication
{
    /// <summary>Un correo = un destinatario por campaña (evita duplicados por contactos repetidos en CRM).</summary>
    public static List<ContactEntity> ByEmail(IEnumerable<ContactEntity> contacts)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<ContactEntity>();

        foreach (var contact in contacts)
        {
            var email = contact.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                continue;

            if (!seen.Add(email))
                continue;

            result.Add(contact);
        }

        return result;
    }
}
