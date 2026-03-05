namespace MiNegocioCR.Api.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using MiNegocioCR.Api.Domain.Entities;

    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.BusinessId);

            builder.HasIndex(x => x.SupplierId);

            builder.HasIndex(x => x.PurchaseDate);
        }
    }
}
