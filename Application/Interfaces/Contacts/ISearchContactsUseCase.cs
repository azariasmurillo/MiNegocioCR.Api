using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface ISearchContactsUseCase
{
    Task<List<ContactResponseDto>> Execute(Guid businessId, string? query);
}
