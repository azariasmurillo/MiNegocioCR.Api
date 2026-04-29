using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IUpdateContactUseCase
{
    Task<ContactResponseDto> Execute(Guid businessId, Guid id, UpdateContactRequestDto request);
}
