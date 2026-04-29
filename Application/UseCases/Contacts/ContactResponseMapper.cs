using MiNegocioCR.Api.Application.DTOs;
namespace MiNegocioCR.Api.Application.UseCases.Contacts;

internal static class ContactResponseMapper
{
    internal static ContactResponseDto ToDto(MiNegocioCR.Api.Domain.Entities.Contact contact) =>
        new()
        {
            Id = contact.Id,
            BusinessId = contact.BusinessId,
            Name = contact.Name,
            Phone = contact.Phone,
            Email = contact.Email,
            CreatedAt = contact.CreatedAt,
            IsDeleted = contact.IsDeleted,
            DeletedAt = contact.DeletedAt
        };
}
