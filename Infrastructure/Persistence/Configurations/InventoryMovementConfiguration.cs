namespace MiNegocioCR.Api.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using MiNegocioCR.Api.Domain.Entities;

    public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
    {
        public void Configure(EntityTypeBuilder<InventoryMovement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.BusinessId);

            builder.HasIndex(x => x.CatalogVariantId);

            builder.HasIndex(x => x.CreatedAt);

            builder.HasIndex(x => new { x.BusinessId, x.CreatedAt });
        }
    }
}
