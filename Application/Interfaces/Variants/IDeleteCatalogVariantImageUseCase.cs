namespace MiNegocioCR.Api.Application.Interfaces.Variants;

public interface IDeleteCatalogVariantImageUseCase
{
    Task ExecuteAsync(Guid businessId, Guid imageId, CancellationToken cancellationToken = default);
}
