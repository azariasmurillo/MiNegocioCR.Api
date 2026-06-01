using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class ContactActivityHelper
{
    public static bool SaleInvolvesPayment(Sale sale)
    {
        if (sale.Total > 0m || sale.PrepaidAmount > 0m)
            return true;

        return sale.PaymentMethods.Any(pm => pm.Amount > 0m);
    }

    public static void Touch(Domain.Entities.Contact contact, DateTime atUtc)
    {
        if (contact.LastActivityAt == null || atUtc > contact.LastActivityAt)
            contact.LastActivityAt = atUtc;
    }
}
