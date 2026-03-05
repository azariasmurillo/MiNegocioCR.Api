

namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface IBusinessRepository
    {
        Task<MiNegocioCR.Api.Domain.Entities.Business?> GetByWhatsappPhoneNumberIdAsync(string phoneNumberId);
    }
}
