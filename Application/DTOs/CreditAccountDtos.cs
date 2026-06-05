using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs;

public class CreditChargeLineInputDto
{
    public string LineKind { get; set; } = "FreeConcept";
    public Guid? CatalogVariantId { get; set; }
    public string ConceptName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal BaseUnitPriceCrc { get; set; }
    public decimal CreditMarkupPercent { get; set; }
    public decimal UnitPriceCrc { get; set; }
}

public class ResolvedCreditChargeLineDto
{
    public int SortOrder { get; set; }
    public CreditTransactionLineKind LineKind { get; set; }
    public Guid? CatalogVariantId { get; set; }
    public string ConceptName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal BaseUnitPriceCrc { get; set; }
    public decimal CreditMarkupPercent { get; set; }
    public decimal UnitPriceCrc { get; set; }
    public decimal LineTotalCrc { get; set; }
}

public class CreateCreditChargeRequestDto
{
    public Guid? ContactId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime? PaymentCommitmentDate { get; set; }
    public string? Notes { get; set; }
    public List<CreditChargeLineInputDto> Lines { get; set; } = new();
}

public class RegisterCreditPaymentRequestDto
{
    public decimal AmountCrc { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCreditCommitmentRequestDto
{
    public DateTime? PaymentCommitmentDate { get; set; }
    public string? Notes { get; set; }
}

public class SendCreditEmailRequestDto
{
    public string HtmlContent { get; set; } = string.Empty;
    public string? DestinationEmail { get; set; }
    public string? Subject { get; set; }
}

public class AddCreditCommunicationRequestDto
{
    public string CommunicationType { get; set; } = "Llamada";
    public string? Notes { get; set; }
}
