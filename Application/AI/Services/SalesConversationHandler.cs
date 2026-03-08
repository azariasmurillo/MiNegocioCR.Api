using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Parsing;
using MiNegocioCR.Api.Application.AI.Sales;
using MiNegocioCR.Api.Application.AI.State;
using MiNegocioCR.Api.Application.AI.Upsell;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.Services
{
    public class SalesConversationHandler : ISalesConversationHandler
    {
        private readonly IConversationStateService _state;
        private readonly ISaleService _saleService;
        private readonly IUpsellService _upsellService;
        private readonly AppDbContext _context;

        public SalesConversationHandler(
            IConversationStateService state,
            ISaleService saleService,
            IUpsellService upsellService,
            AppDbContext context)
        {
            _state = state;
            _saleService = saleService;
            _upsellService = upsellService;
            _context = context;
        }

        public async Task<string?> HandleAsync(
            Guid businessId,
            string phoneNumber,
            string normalizedMessage,
            ConversationState? conversationState)
        {
            if (conversationState == null)
                return null;

            if (conversationState.Step == "awaiting_confirmation")
            {
                var quantityInMessage = QuantityParser.TryParseQuantity(normalizedMessage);
                var isAffirmative = IsAffirmativeConfirmation(normalizedMessage);

                if (quantityInMessage.HasValue && quantityInMessage.Value >= 1 && isAffirmative)
                {
                    var result = await _saleService.CreateSaleAsync(
                        businessId,
                        conversationState.ProductId!.Value,
                        phoneNumber,
                        quantityInMessage.Value);

                    result = await AppendUpsellOfferAsync(businessId, conversationState.ProductId.Value, result);
                    await _state.ClearAsync(businessId, phoneNumber);
                    return result;
                }

                if (normalizedMessage.Contains("si") || normalizedMessage.Contains("sí"))
                {
                    conversationState.Step = "awaiting_quantity";
                    await _state.SaveAsync(conversationState);
                    return "Perfecto. ¿Cuántas unidades deseas comprar?";
                }

                if (normalizedMessage.Contains("no"))
                {
                    await _state.ClearAsync(businessId, phoneNumber);
                    return "Compra cancelada.";
                }
            }

            if (conversationState.Step == "awaiting_quantity")
            {
                var quantity = QuantityParser.TryParseQuantity(normalizedMessage);
                if (quantity.HasValue && quantity.Value >= 1)
                {
                    var result = await _saleService.CreateSaleAsync(
                        businessId,
                        conversationState.ProductId!.Value,
                        phoneNumber,
                        quantity.Value);

                    result = await AppendUpsellOfferAsync(businessId, conversationState.ProductId.Value, result);
                    await _state.ClearAsync(businessId, phoneNumber);
                    return result;
                }

                return "No entendí cuántas unidades. Por favor escribe un número, por ejemplo: 2 o 3.";
            }

            return null;
        }

        private static bool IsAffirmativeConfirmation(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            message = message.ToLower();
            var patterns = new[]
            {
                "si", "sí", "claro", "ok", "dale", "perfecto", "me llevo", "voy a comprar",
                "quiero comprar", "porque no", "de una", "hagale", "hágale", "listo",
                "está bien", "esta bien"
            };
            return patterns.Any(p => message.Contains(p));
        }

        private async Task<string> AppendUpsellOfferAsync(Guid businessId, Guid variantId, string message)
        {
            var variant = await _context.CatalogVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == variantId);
            if (variant == null) return message;

            var upsells = await _upsellService.GetUpsell(businessId, variant.CatalogItemId);
            if (upsells.Any())
            {
                var suggestion = upsells.First();
                return message + $"\n\nTambién podrías agregar {suggestion.Name} por ₡{suggestion.BasePrice:N0}. ¿Deseas agregarlo?";
            }

            var fallback = await _upsellService.GetFallbackUpsell(businessId);
            if (fallback.Any())
            {
                var sb = new System.Text.StringBuilder(message);
                sb.Append("\n\nTambién podrías agregar:\n");
                foreach (var item in fallback.Take(2))
                    sb.Append($"• {item.Name} ₡{item.BasePrice:N0}\n");
                return sb.ToString();
            }

            return message;
        }
    }
}
