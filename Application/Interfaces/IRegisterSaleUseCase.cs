using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces
{
    namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Sales
    {
        public interface IRegisterSaleUseCase
        {
            Task<object> ExecuteAsync(
                CreateSaleRequestDto request,
                string? customerPhone = null,
                string? customerName = null,
                string? customerEmail = null);
        }
    }
}
