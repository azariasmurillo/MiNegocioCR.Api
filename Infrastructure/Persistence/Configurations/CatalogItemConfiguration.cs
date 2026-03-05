namespace MiNegocioCR.Api.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using MiNegocioCR.Api.Domain.Entities;

    public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.BusinessId);

            builder.HasIndex(x => new { x.BusinessId, x.CategoryId });

            builder.HasIndex(x => new { x.BusinessId, x.IsActive });
        }
    } 
}
