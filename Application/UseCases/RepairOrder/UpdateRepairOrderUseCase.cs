using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder
{
    public class UpdateRepairOrderUseCase : IUpdateRepairOrderUseCase
    {
        private readonly IAppDbContext _context;
        private readonly IGetRepairOrderByIdUseCase _getById;

        public UpdateRepairOrderUseCase(
            IAppDbContext context,
            IGetRepairOrderByIdUseCase getById)
        {
            _context = context;
            _getById = getById;
        }

        public async Task<object> Execute(Guid businessId, Guid id, UpdateRepairOrderRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var order = await _context.RepairOrders
                .AsTracking()
                .Include(o => o.Contact)
                .FirstOrDefaultAsync(o => o.BusinessId == businessId && o.Id == id);

            if (order == null)
                throw new NotFoundException("RepairOrder", "Order not found");

            if ((RepairOrderStatus)order.Status == RepairOrderStatus.Delivered)
                throw new ArgumentException("Delivered orders cannot be modified (including line items).");

            if ((RepairOrderStatus)order.Status == RepairOrderStatus.Cancelled)
                throw new ArgumentException("Cancelled orders cannot be modified (including line items).");

            if (request.Items != null)
            {
                foreach (var it in request.Items)
                    RepairOrderItemsRequestHelper.ValidateLine(it);
                await RepairOrderItemsRequestHelper.ValidateVariantIdsForBusinessAsync(
                    _context,
                    order.BusinessId,
                    request.Items,
                    CancellationToken.None);

                // Reemplazo total de líneas en SQL directo para evitar conflictos
                // de tracking/concurrencia al borrar y reinsertar en el mismo contexto.
                await _context.RepairOrderItems
                    .Where(i => i.RepairOrderId == order.Id)
                    .ExecuteDeleteAsync();

                if (request.Items.Count > 0)
                {
                    var newItems = RepairOrderItemsRequestHelper.MapToNewEntities(order, request.Items);
                    _context.RepairOrderItems.AddRange(newItems);
                }
            }

            if (request.ContactId.HasValue)
            {
                var c = await _context.Contacts
                    .FirstOrDefaultAsync(
                        x => x.Id == request.ContactId.Value && x.BusinessId == order.BusinessId);
                if (c == null)
                    throw new NotFoundException("Contact", "El contacto no existe o no pertenece a este negocio.");
                order.ContactId = c.Id;
                order.Contact = c;
                if (!string.IsNullOrWhiteSpace(request.Name))
                    c.Name = request.Name.Trim();
                if (request.Email != null)
                    c.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
                var phoneWhenId = PhoneSanitizer.Sanitize(request.Phone);
                if (!string.IsNullOrWhiteSpace(phoneWhenId) && phoneWhenId != c.Phone)
                {
                    var nameForPhone = !string.IsNullOrWhiteSpace(request.Name)
                        ? request.Name.Trim()
                        : c.Name;
                    var resolved = await RepairOrderContactHelper.GetOrCreateContactAsync(
                        _context,
                        order.BusinessId,
                        nameForPhone,
                        request.Phone,
                        request.Email ?? c.Email);
                    order.ContactId = resolved.Id;
                    order.Contact = resolved;
                }
            }
            else
            {
                var newPhone = PhoneSanitizer.Sanitize(request.Phone);
                if (!string.IsNullOrWhiteSpace(newPhone))
                {
                    var nameForRow = !string.IsNullOrWhiteSpace(request.Name)
                        ? request.Name!.Trim()
                        : order.Contact.Name;
                    if (string.IsNullOrWhiteSpace(nameForRow))
                        throw new ArgumentException("Se requiere el nombre del cliente o un ContactId.", nameof(request.Name));
                    var resolved = await RepairOrderContactHelper.GetOrCreateContactAsync(
                        _context,
                        order.BusinessId,
                        nameForRow,
                        request.Phone,
                        request.Email);
                    order.ContactId = resolved.Id;
                    order.Contact = resolved;
                }
                else if (request.Name != null || request.Email != null)
                {
                    if (request.Name != null)
                    {
                        if (string.IsNullOrWhiteSpace(request.Name))
                            throw new ArgumentException("Name cannot be empty when provided.", nameof(request.Name));
                        order.Contact.Name = request.Name.Trim();
                    }
                    if (request.Email != null)
                        order.Contact.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
                }
            }

            order.ProblemDescription = request.ProblemDescription;
            order.DeviceType = request.DeviceType;
            order.DeviceTypeOther = request.DeviceTypeOther;
            order.Brand = request.Brand;
            order.Model = request.Model;
            order.SerialNumber = request.SerialNumber;
            order.AccessoriesIncluded = request.AccessoriesIncluded;
            order.OperatingSystem = request.OperatingSystem;
            order.Password = request.Password;
            if (request.IsDiagnosticPaid.HasValue)
                order.IsDiagnosticPaid = request.IsDiagnosticPaid.Value;
            if (request.DiscountPercent.HasValue)
                order.DiscountPercent = request.DiscountPercent.Value;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);

            var result = await _getById.Execute(businessId, id);
            if (result == null)
                throw new InvalidOperationException("La orden no pudo leerse luego de actualizarse.");
            return result;
        }
    }
}
