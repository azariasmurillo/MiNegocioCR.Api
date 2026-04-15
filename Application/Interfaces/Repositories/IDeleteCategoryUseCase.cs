namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IDeleteCategoryUseCase
    {
        Task ExecuteAsync(Guid categoryId, Guid businessId);
    }
}
