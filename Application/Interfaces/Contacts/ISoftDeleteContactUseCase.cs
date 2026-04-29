using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface ISoftDeleteContactUseCase
{
    Task<ContactResponseDto> Execute(Guid businessId, Guid id);
}
