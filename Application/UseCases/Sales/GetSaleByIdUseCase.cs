using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

public class GetSaleByIdUseCase : IGetSaleByIdUseCase
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleByIdUseCase(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<object?> ExecuteAsync(Guid saleId, Guid? businessId = null)
    {
        var sale = await _saleRepository.GetSaleByIdAsync(saleId);
        if (sale is null)
            return null;

        if (businessId.HasValue && businessId.Value != Guid.Empty && sale.BusinessId != businessId.Value)
            return null;

        var contact = sale.Contact;

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
                i.Description,
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
}
