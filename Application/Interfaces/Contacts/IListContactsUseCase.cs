using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IListContactsUseCase
{
    Task<List<ContactResponseDto>> Execute(Guid businessId);
}
