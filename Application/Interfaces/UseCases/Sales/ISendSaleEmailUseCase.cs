namespace MiNegocioCR.Api.Application.Interfaces
{
    namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Sales
    {
        public interface ISendSaleEmailUseCase
        {
            Task Execute(Guid id, string htmlContent, string? destinationEmail = null);
        }
    }
}
