using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class AuthSessionMapper
{
    public static AuthSessionDto ToDto(User user, string token)
    {
        var hasBusiness = user.BusinessId != Guid.Empty;

        return new AuthSessionDto
        {
            UserId = user.Id,
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? string.Empty : user.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(user.Email) ? string.Empty : user.Email.Trim(),
            Role = string.IsNullOrWhiteSpace(user.Role) ? string.Empty : user.Role.Trim(),
            IsActive = user.IsActive,
            BusinessId = hasBusiness ? user.BusinessId : null,
            BusinessName = hasBusiness ? user.Business?.Name : null,
            Token = token
        };
    }
}
