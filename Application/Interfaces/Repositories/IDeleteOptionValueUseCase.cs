namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IDeleteOptionValueUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
