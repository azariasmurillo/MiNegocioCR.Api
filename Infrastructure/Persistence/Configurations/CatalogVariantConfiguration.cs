namespace MiNegocioCR.Api.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using MiNegocioCR.Api.Domain.Entities;

    public class CatalogVariantConfiguration : IEntityTypeConfiguration<CatalogVariant>
    {
        public void Configure(EntityTypeBuilder<CatalogVariant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.CatalogItemId);

            builder.HasIndex(x => x.SKU);
        }
    }
}
