using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Payments;

public class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IAppDbContext _context;

    public CreatePaymentUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<object> Execute(CreatePaymentRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.BusinessId == Guid.Empty) throw new ArgumentException("BusinessId is required.", nameof(request.BusinessId));
        if (request.RepairOrderId == Guid.Empty) throw new ArgumentException("RepairOrderId is required.", nameof(request.RepairOrderId));
        if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));

        var order = await _context.RepairOrders
            .FirstOrDefaultAsync(x => x.Id == request.RepairOrderId && x.BusinessId == request.BusinessId);

        if (order is null)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        if ((RepairOrderStatus)order.Status == RepairOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot register payments for cancelled repair orders.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BusinessId = request.BusinessId,
            RepairOrderId = request.RepairOrderId,
            Amount = Math.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Type = request.Type,
            Method = request.Method,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(CancellationToken.None);

        return new
        {
            payment.Id,
            payment.BusinessId,
            payment.RepairOrderId,
            payment.Amount,
            payment.Type,
            payment.Method,
            payment.Notes,
            payment.CreatedAt
        };
    }
}
