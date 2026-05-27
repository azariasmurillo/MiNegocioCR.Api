using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly AppDbContext _context;

        public SaleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));
            await _context.Sales.AddAsync(sale);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Sale>> GetSalesAsync(Guid businessId)
        {
            return await _context.Sales
                .Where(x => x.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleAsync(Guid id, Guid businessId)
        {
            return await _context.Sales
                .Include(x => x.Items)
                .Include(x => x.PaymentMethods)
                .FirstOrDefaultAsync(x => x.Id == id && x.BusinessId == businessId);
        }

        public async Task<Sale?> GetSaleByIdAsync(Guid id)
        {
            return await _context.Sales
                .AsNoTracking()
                .Include(x => x.Items)
                .Include(x => x.Contact)
                .Include(x => x.PaymentMethods)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedResultDto<SalesListItemDto>> GetPagedSalesAsync(Guid businessId, SalesListQueryDto query)
        {
            var salesQuery = _context.Sales
                .AsNoTracking()
                .Include(x => x.PaymentMethods)
                .Where(x => x.BusinessId == businessId);

            if (query.From.HasValue)
                salesQuery = salesQuery.Where(x => x.CreatedAt >= query.From.Value);

            if (query.ToExclusive.HasValue)
                salesQuery = salesQuery.Where(x => x.CreatedAt < query.ToExclusive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                salesQuery = salesQuery.Where(x =>
                    x.InvoiceNumber.ToLower().Contains(search) ||
                    (x.CustomerPhone != null && x.CustomerPhone.ToLower().Contains(search)) ||
                    (x.Contact != null && x.Contact.Name != null && x.Contact.Name.ToLower().Contains(search)) ||
                    (x.Contact != null && x.Contact.Phone != null && x.Contact.Phone.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
            {
                var methodLower = query.PaymentMethod.Trim().ToLower();
                var method = methodLower switch
                {
                    "cash" or "efectivo" => (int?)PaymentMethod.Cash,
                    "transfer" or "transferencia" => (int?)PaymentMethod.Transfer,
                    "sinpe" => (int?)PaymentMethod.Sinpe,
                    "card" or "tarjeta" => (int?)PaymentMethod.Card,
                    _ => null,
                };
                if (method.HasValue)
                {
                    salesQuery = salesQuery.Where(x => x.PaymentMethods.Any(pm => (int)pm.Method == method.Value));
                }
            }

            if (query.FromRepair == true)
                salesQuery = salesQuery.Where(x => x.RepairOrderId != null);

            var sort = query.Sort?.Trim().ToLower() ?? "createdat desc";
            salesQuery = sort switch
            {
                "createdat asc"       => salesQuery.OrderBy(x => x.CreatedAt),
                "total asc"           => salesQuery.OrderBy(x => x.Total),
                "total desc"          => salesQuery.OrderByDescending(x => x.Total),
                "invoicenumber asc"   => salesQuery.OrderBy(x => x.InvoiceNumber),
                "invoicenumber desc"  => salesQuery.OrderByDescending(x => x.InvoiceNumber),
                _                     => salesQuery.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await salesQuery.CountAsync();
            var skip = (query.Page - 1) * query.PageSize;

            var items = await salesQuery
                .Skip(skip)
                .Take(query.PageSize)
                .Select(x => new SalesListItemDto
                {
                    Id = x.Id,
                    InvoiceNumber = x.InvoiceNumber,
                    CreatedAt = x.CreatedAt,
                    CustomerName = x.Contact != null ? x.Contact.Name : null,
                    CustomerPhone = x.Contact != null && x.Contact.Phone != null && x.Contact.Phone != ""
                        ? x.Contact.Phone
                        : (string.IsNullOrWhiteSpace(x.CustomerPhone) ? null : x.CustomerPhone),
                    RepairOrderId = x.RepairOrderId,
                    Source = x.Source,
                    TaxAmount = x.TaxAmount,
                    TotalProfit = x.TotalProfit,
                    Total = x.Total,
                    TotalOrden = x.TotalOrden,
                    PrepaidAmount = x.PrepaidAmount,
                    Subtotal = x.Subtotal,
                    DiscountAmount = x.DiscountAmount,
                    DiscountKind = ((Domain.Enums.SaleDiscountKind)x.DiscountKind).ToString(),
                    DiscountInputValue = x.DiscountInputValue,
                    PaymentMethods = x.PaymentMethods
                        .Select(pm => new SalePaymentMethodDto
                        {
                            Method = pm.Method.ToString(),
                            Amount = pm.Amount
                        })
                        .ToList()
                })
                .ToListAsync();

            return new PagedResultDto<SalesListItemDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
