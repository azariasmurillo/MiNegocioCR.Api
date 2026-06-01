using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class ListContactInsightsUseCase : IListContactInsightsUseCase
{
    private readonly IAppDbContext _context;

    public ListContactInsightsUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContactInsightsResultDto> Execute(
        Guid businessId,
        int inactiveDays = 60,
        bool? inactiveOnly = null,
        bool? hasEmailOnly = null,
        string? search = null)
    {
        if (inactiveDays < 1)
            inactiveDays = 60;

        var thresholdUtc = DateTime.UtcNow.AddDays(-inactiveDays);

        var contactsQuery = _context.Contacts
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            contactsQuery = contactsQuery.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Phone.ToLower().Contains(term) ||
                (c.Email != null && c.Email.ToLower().Contains(term)));
        }

        if (hasEmailOnly == true)
            contactsQuery = contactsQuery.Where(c => c.Email != null && c.Email.Trim() != "");

        var contacts = await contactsQuery
            .OrderBy(c => c.Name)
            .Select(c => new ContactInsightResponseDto
            {
                Id = c.Id,
                BusinessId = c.BusinessId,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                CreatedAt = c.CreatedAt,
                LastActivityAt = c.LastActivityAt,
                HasEmail = c.Email != null && c.Email.Trim() != ""
            })
            .ToListAsync();

        if (contacts.Count == 0)
        {
            return new ContactInsightsResultDto
            {
                Summary = new ContactInsightsSummaryDto
                {
                    InactiveDaysThreshold = inactiveDays
                }
            };
        }

        var contactIds = contacts.Select(c => c.Id).ToList();

        var salesAgg = await _context.Sales
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId && s.ContactId != null && contactIds.Contains(s.ContactId.Value))
            .GroupBy(s => s.ContactId!.Value)
            .Select(g => new
            {
                ContactId = g.Key,
                PurchaseCount = g.Count(),
                InvoicedTotal = g.Sum(s => s.TotalOrden)
            })
            .ToListAsync();

        var openPaymentAgg = await _context.Payments
            .AsNoTracking()
            .Where(p => p.BusinessId == businessId && p.Amount > 0)
            .Join(
                _context.RepairOrders.AsNoTracking().Where(r => !r.IsInvoiced),
                p => p.RepairOrderId,
                r => r.Id,
                (p, r) => new { r.ContactId, p.Amount })
            .Where(x => contactIds.Contains(x.ContactId))
            .GroupBy(x => x.ContactId)
            .Select(g => new
            {
                ContactId = g.Key,
                OpenPaymentsTotal = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        var salesByContact = salesAgg.ToDictionary(x => x.ContactId);
        var openPaymentsByContact = openPaymentAgg.ToDictionary(x => x.ContactId);

        var nowUtc = DateTime.UtcNow;

        foreach (var contact in contacts)
        {
            if (salesByContact.TryGetValue(contact.Id, out var sales))
            {
                contact.PurchaseCount = sales.PurchaseCount;
                contact.TotalSpent = sales.InvoicedTotal;
            }

            if (openPaymentsByContact.TryGetValue(contact.Id, out var openPayments))
                contact.TotalSpent += openPayments.OpenPaymentsTotal;

            contact.HasNeverPaid = contact.LastActivityAt == null;
            contact.DaysSinceActivity = contact.LastActivityAt.HasValue
                ? Math.Max(0, (int)Math.Floor((nowUtc - contact.LastActivityAt.Value).TotalDays))
                : null;
            contact.IsInactive = contact.LastActivityAt == null || contact.LastActivityAt < thresholdUtc;
        }

        var filtered = contacts;
        if (inactiveOnly == true)
            filtered = filtered.Where(c => c.IsInactive).ToList();

        var summary = new ContactInsightsSummaryDto
        {
            TotalContacts = contacts.Count,
            WithEmail = contacts.Count(c => c.HasEmail),
            ActiveCount = contacts.Count(c => !c.IsInactive),
            InactiveCount = contacts.Count(c => c.IsInactive),
            NeverPaidCount = contacts.Count(c => c.HasNeverPaid),
            InactiveDaysThreshold = inactiveDays
        };

        return new ContactInsightsResultDto
        {
            Summary = summary,
            Contacts = filtered
        };
    }
}
