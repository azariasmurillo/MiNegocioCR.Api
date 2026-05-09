using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
