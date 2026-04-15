namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IDeleteCatalogItemUseCase
    {
        Task ExecuteAsync(Guid id, Guid businessId);
    }
}
