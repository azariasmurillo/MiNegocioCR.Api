using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class UpdateStatusRequestDto
    {
        public RepairOrderStatus NewStatus { get; set; }
    }
}
