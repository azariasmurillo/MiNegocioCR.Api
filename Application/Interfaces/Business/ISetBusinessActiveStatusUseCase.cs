namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface ISetBusinessActiveStatusUseCase
    {
        Task Execute(Guid businessId, bool isActive);
    }
}
