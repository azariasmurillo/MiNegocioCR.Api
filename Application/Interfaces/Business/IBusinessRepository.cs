using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Application.Interfaces.Business
{
    public interface IBusinessRepository
    {
        Task<BusinessEntity?> GetByWhatsappPhoneNumberIdAsync(string phoneNumberId);
    }
}
