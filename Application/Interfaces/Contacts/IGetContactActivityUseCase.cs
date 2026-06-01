using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IGetContactActivityUseCase
{
    Task<ContactActivityResultDto?> Execute(Guid businessId, Guid contactId, int take = 15);
}
