
namespace MiNegocioCR.Api.Aplication.Interfaces.Business
{
    public interface IBusinessRepository
    {
        Task<MiNegocioCR.Api.Domain.Entities.Business?> GetByWhatsappPhoneNumberIdAsync(string phoneNumberId);
    }
}
