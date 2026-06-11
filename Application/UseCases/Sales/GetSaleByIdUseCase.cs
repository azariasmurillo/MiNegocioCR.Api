using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

public class GetSaleByIdUseCase : IGetSaleByIdUseCase
{
    private readonly ISaleRepository _saleRepository;
    private readonly IAppDbContext _context;

    public GetSaleByIdUseCase(ISaleRepository saleRepository, IAppDbContext context)
    {
        _saleRepository = saleRepository;
        _context = context;
    }

    public async Task<object?> ExecuteAsync(Guid saleId, Guid? businessId = null)
    {
        var sale = await _saleRepository.GetSaleByIdAsync(saleId);
        if (sale is null)
            return null;

        if (businessId.HasValue && businessId.Value != Guid.Empty && sale.BusinessId != businessId.Value)
            return null;

        var contact = sale.Contact;
        var variantLabels = await LoadVariantDescriptionsAsync(sale.Items);

        return new
        {
            sale.Id,
            sale.BusinessId,
            sale.InvoiceNumber,
            sale.HaciendaConsecutive,
            sale.RepairOrderId,
            sale.Source,
            sale.SaleDate,
            sale.CreatedAt,
            sale.CustomerPhone,
            CustomerName = contact?.Name,
            CustomerEmail = contact?.Email,
            Contact = contact == null ? null : new
            {
                contact.Id,
                contact.Name,
                contact.Phone,
                contact.Email
            },
            Items = sale.Items.OrderBy(i => i.Id).Select(i => new
            {
                i.Id,
                i.ItemType,
                i.CatalogVariantId,
                Description = ResolveItemDescription(i.Description, i.CatalogVariantId, variantLabels),
                i.Quantity,
                i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }),
            Totals = new
            {
                sale.Subtotal,
                Discount = sale.DiscountAmount,
                sale.DiscountAmount,
                DiscountKind = ((Domain.Enums.SaleDiscountKind)sale.DiscountKind).ToString(),
                DiscountInputValue = sale.DiscountInputValue,
                Tax = sale.TaxAmount,
                sale.TaxAmount,
                TotalOrden = sale.TotalOrden,
                PrepaidAmount = sale.PrepaidAmount,
                Total = sale.Total
            },
            PaymentMethods = sale.PaymentMethods.Select(pm => new
            {
                Method = pm.Method.ToString(),
                pm.Amount
            })
        };
    }

    private static string? ResolveItemDescription(
        string? stored,
        Guid? catalogVariantId,
        IReadOnlyDictionary<Guid, string> variantLabels)
    {
        if (!string.IsNullOrWhiteSpace(stored))
            return stored.Trim();

        if (catalogVariantId.HasValue && variantLabels.TryGetValue(catalogVariantId.Value, out var label))
            return label;

        return stored;
    }

    private async Task<Dictionary<Guid, string>> LoadVariantDescriptionsAsync(
        IEnumerable<Domain.Entities.SaleItem> items)
    {
        var variantIds = items
            .Where(i => i.CatalogVariantId.HasValue && string.IsNullOrWhiteSpace(i.Description))
            .Select(i => i.CatalogVariantId!.Value)
            .Distinct()
            .ToList();
        if (variantIds.Count == 0)
            return new Dictionary<Guid, string>();

        var variants = await _context.CatalogVariants
            .AsNoTracking()
            .Where(v => variantIds.Contains(v.Id))
            .Include(v => v.CatalogItem)
            .Include(v => v.VariantOptionValues)
                .ThenInclude(l => l.CatalogOptionValue)
                    .ThenInclude(ov => ov.CatalogOption)
            .ToListAsync();

        return variants.ToDictionary(v => v.Id, SaleItemDescriptionResolver.BuildFromVariant);
    }
}
