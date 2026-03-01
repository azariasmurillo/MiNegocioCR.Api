namespace MiNegocioCR.Api.Aplication.Interfaces.Business
{
    public interface ISetBusinessActiveStatusUseCase
    {
        Task Execute(Guid businessId, bool isActive);
    }
}
