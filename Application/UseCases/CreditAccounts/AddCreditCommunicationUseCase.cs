using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class AddCreditCommunicationUseCase : IAddCreditCommunicationUseCase
{
    private readonly IAppDbContext _context;
    private readonly IGetCreditAccountByIdUseCase _getById;

    public AddCreditCommunicationUseCase(IAppDbContext context, IGetCreditAccountByIdUseCase getById)
    {
        _context = context;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, Guid accountId, AddCreditCommunicationRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? null
            : request.Notes.Trim();

        if (notes == null)
            throw new ArgumentException("Indicá una observación del seguimiento.", nameof(request));

        var account = await _context.CreditAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.BusinessId == businessId)
            ?? throw new NotFoundException("CreditAccount", "Cuenta de crédito no encontrada.");

        if (account.Status == (int)CreditAccountStatus.Cancelled)
            throw new InvalidOperationException("La cuenta está cancelada.");

        var type = ParseCommunicationType(request.CommunicationType);

        _context.CreditCommunications.Add(new CreditCommunication
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CreditAccountId = accountId,
            ContactId = account.ContactId,
            CommunicationType = (int)type,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
        });

        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _getById.Execute(businessId, account.Id);
        return result ?? throw new InvalidOperationException("No se pudo leer la cuenta actualizada.");
    }

    private static CreditCommunicationType ParseCommunicationType(string? raw)
    {
        var v = (raw ?? string.Empty).Trim();
        if (v.Equals("Correo", StringComparison.OrdinalIgnoreCase)) return CreditCommunicationType.Correo;
        if (v.Equals("Llamada", StringComparison.OrdinalIgnoreCase)) return CreditCommunicationType.Llamada;
        if (v.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase)) return CreditCommunicationType.WhatsApp;
        if (v.Equals("Visita", StringComparison.OrdinalIgnoreCase)) return CreditCommunicationType.Visita;
        if (v.Equals("Otro", StringComparison.OrdinalIgnoreCase)) return CreditCommunicationType.Otro;
        throw new ArgumentException("Tipo de comunicación inválido. Usá: Correo, Llamada, WhatsApp, Visita u Otro.");
    }
}
