using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts
{
    public interface IContact
    {
        Task ImportContactsAsync(Guid businessId, List<ContactDto> contacts);
        Task<List<ContactDto>> GetContactsAsync(Guid businessId);
    }
}
