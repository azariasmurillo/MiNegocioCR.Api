using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<BusinessEntity> Businesses { get; }
        DbSet<RepairOrder> RepairOrders { get; }
        DbSet<User> Users { get; }
        DbSet<BusinessSettings> BusinessSettings { get; }
        DbSet<Domain.Entities.WhatsappMessage> WhatsappMessages { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
