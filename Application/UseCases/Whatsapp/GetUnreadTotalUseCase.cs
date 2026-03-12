using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.Application.UseCases.Whatsapp
{
    public class GetUnreadTotalUseCase : IGetUnreadTotalUseCase
    {
        private readonly IWhatsappMessageRepository _repository;

        public GetUnreadTotalUseCase(IWhatsappMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Execute(Guid businessId)
        {
            return await _repository.GetUnreadTotalAsync(businessId);
        }
    }
}
