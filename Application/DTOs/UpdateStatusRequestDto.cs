using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateStatusRequestDto
    {
        public RepairOrderStatus NewStatus { get; set; }
    }
}
