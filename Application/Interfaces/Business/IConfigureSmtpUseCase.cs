using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface IConfigureSmtpUseCase
    {
        Task Execute(Guid businessId, ConfigureSmtpDto dto);
    }
}
