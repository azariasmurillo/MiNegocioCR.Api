using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.CreditAccounts;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.CreditAccounts;

public class SendCreditAccountEmailUseCase : ISendCreditAccountEmailUseCase
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public SendCreditAccountEmailUseCase(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Execute(Guid businessId, Guid accountId, SendCreditEmailRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.HtmlContent))
            throw new ArgumentException("El contenido HTML es obligatorio.", nameof(request));

        var account = await _context.CreditAccounts
            .Include(a => a.Contact)
            .FirstOrDefaultAsync(a => a.Id == accountId && a.BusinessId == businessId)
            ?? throw new NotFoundException("CreditAccount", "Cuenta de crédito no encontrada.");

        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId)
            ?? throw new NotFoundException("Business", "Negocio no encontrado.");

        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Las notificaciones por correo están desactivadas para este negocio.");

        var toEmail = string.IsNullOrWhiteSpace(request.DestinationEmail)
            ? account.Contact.Email
            : request.DestinationEmail.Trim();

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("El contacto no tiene correo electrónico.");

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? $"Cuenta de crédito {account.AccountNumber}"
            : request.Subject.Trim();

        await _emailService.SendAsync(business, toEmail, subject, request.HtmlContent);

        _context.CreditCommunications.Add(new CreditCommunication
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CreditAccountId = accountId,
            ContactId = account.ContactId,
            CommunicationType = (int)CreditCommunicationType.Correo,
            Notes = $"Correo manual: {subject}",
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
