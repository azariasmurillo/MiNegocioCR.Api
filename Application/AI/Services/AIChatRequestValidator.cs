using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Application.AI.Services
{
    public class AIChatRequestValidator : IAIChatRequestValidator
    {
        private readonly IAppDbContext _context;

        private static readonly string[] CourtesyWords =
        {
            "gracias", "thank", "ok", "perfecto", "listo", "dale", "pura vida", "👍"
        };

        private const string CourtesyResponse =
            "¡Con gusto! Si necesitas información sobre productos, accesorios o reparaciones, aquí estoy para ayudarte.";

        public AIChatRequestValidator(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<AIChatValidationResult> ValidateAsync(AIRequest request)
        {
            var businessExists = await _context.Businesses.AnyAsync(b => b.Id == request.BusinessId);
            if (!businessExists)
                return AIChatValidationResult.EarlyExit("Negocio no encontrado.");

            var settings = await _context.BusinessSettings
                .FirstOrDefaultAsync(x => x.BusinessId == request.BusinessId);

            if (settings != null && !settings.EnableAIChat)
                return AIChatValidationResult.EarlyExit("");

            var normalizedMessage = request.UserMessage.Trim().ToLower();

            if (normalizedMessage.Length < 20 &&
                CourtesyWords.Any(w => normalizedMessage.Contains(w)))
            {
                return AIChatValidationResult.EarlyExit(CourtesyResponse, normalizedMessage);
            }

            return AIChatValidationResult.Continue(normalizedMessage);
        }
    }
}
