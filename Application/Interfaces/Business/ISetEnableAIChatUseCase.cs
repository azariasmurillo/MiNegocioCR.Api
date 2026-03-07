namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface ISetEnableAIChatUseCase
    {
        Task ExecuteAsync(Guid businessId, bool enable);
    }
}
