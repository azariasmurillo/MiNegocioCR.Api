using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.Business
{
    public interface IConfigureSmtpUseCase
    {
        Task Execute(Guid businessId, ConfigureSmtpDto dto);
    }
}
