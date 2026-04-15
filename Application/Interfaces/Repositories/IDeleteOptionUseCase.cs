namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IDeleteOptionUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
