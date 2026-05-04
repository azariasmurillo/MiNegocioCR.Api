using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

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
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.BusinessId == businessId);
        }

        public async Task<Sale?> GetSaleByIdAsync(Guid id)
        {
            return await _context.Sales
                .AsNoTracking()
                .Include(x => x.Items)
                .Include(x => x.Contact)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedResultDto<SalesListItemDto>> GetPagedSalesAsync(Guid businessId, SalesListQueryDto query)
        {
            var salesQuery = _context.Sales
                .AsNoTracking()
                .Where(x => x.BusinessId == businessId);

            if (query.From.HasValue)
            {
                var from = query.From.Value.Date;
                salesQuery = salesQuery.Where(x => x.CreatedAt >= from);
            }

            if (query.To.HasValue)
            {
                var toExclusive = query.To.Value.Date.AddDays(1);
                salesQuery = salesQuery.Where(x => x.CreatedAt < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                salesQuery = salesQuery.Where(x =>
                    x.InvoiceNumber.ToLower().Contains(search) ||
                    (x.Contact != null && x.Contact.Name != null && x.Contact.Name.ToLower().Contains(search)) ||
                    (x.Contact != null && x.Contact.Phone != null && x.Contact.Phone.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
            {
                salesQuery = query.PaymentMethod.Trim().ToLower() switch
                {
                    "cash" => salesQuery.Where(x => x.PayCash),
                    "transfer" => salesQuery.Where(x => x.PayTransfer),
                    "sinpe" => salesQuery.Where(x => x.PaySinpe),
                    "card" => salesQuery.Where(x => x.PayCard),
                    _ => salesQuery
                };
            }

            var sort = query.Sort?.Trim().ToLower() ?? "createdat desc";
            salesQuery = sort switch
            {
                "createdat asc" => salesQuery.OrderBy(x => x.CreatedAt),
                "total asc" => salesQuery.OrderBy(x => x.Total > 0 ? x.Total : x.TotalAmount),
                "total desc" => salesQuery.OrderByDescending(x => x.Total > 0 ? x.Total : x.TotalAmount),
                "invoicenumber asc" => salesQuery.OrderBy(x => x.InvoiceNumber),
                "invoicenumber desc" => salesQuery.OrderByDescending(x => x.InvoiceNumber),
                _ => salesQuery.OrderByDescending(x => x.CreatedAt)
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
                    CustomerPhone = x.Contact != null ? x.Contact.Phone : null,
                    Total = x.Total > 0 ? x.Total : x.TotalAmount,
                    PayCash = x.PayCash,
                    PayTransfer = x.PayTransfer,
                    PaySinpe = x.PaySinpe,
                    PayCard = x.PayCard
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
