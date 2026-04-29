namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IHardDeleteContactUseCase
{
    Task Execute(Guid businessId, Guid id);
}
