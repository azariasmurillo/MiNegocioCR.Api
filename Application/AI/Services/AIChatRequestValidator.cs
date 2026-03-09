using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Application.AI.Services
{
    public class AIChatRequestValidator : IAIChatRequestValidator
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<AIChatRequestValidator> _logger;

        private static readonly string[] CourtesyWords =
        {
            "gracias", "thank", "ok", "perfecto", "listo", "dale", "pura vida", "👍"
        };

        private const string CourtesyResponse =
            "¡Con gusto! Si necesitas información sobre productos, accesorios o reparaciones, aquí estoy para ayudarte.";

        public AIChatRequestValidator(IAppDbContext context, ILogger<AIChatRequestValidator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AIChatValidationResult> ValidateAsync(AIRequest request)
        {
            try
            {
                _logger.LogDebug("[ValidateAsync] Inicio. BusinessId: {BusinessId}, UserMessage null: {IsNull}, Len: {Len}",
                    request?.BusinessId, request?.UserMessage == null, request?.UserMessage?.Length ?? -1);

                if (request == null)
                {
                    _logger.LogWarning("[ValidateAsync] Request es null.");
                    return AIChatValidationResult.EarlyExit("Solicitud inválida.");
                }

                var businessExists = await _context.Businesses.AnyAsync(b => b.Id == request.BusinessId);
                _logger.LogDebug("[ValidateAsync] BusinessExists: {Exists}", businessExists);
                if (!businessExists)
                {
                    _logger.LogInformation("[ValidateAsync] Salida: negocio no encontrado.");
                    return AIChatValidationResult.EarlyExit("Negocio no encontrado.");
                }

                var settings = await _context.BusinessSettings
                    .FirstOrDefaultAsync(x => x.BusinessId == request.BusinessId);
                _logger.LogDebug("[ValidateAsync] Settings: {HasSettings}, EnableAIChat: {Enable}",
                    settings != null, settings?.EnableAIChat ?? false);
                if (settings != null && !settings.EnableAIChat)
                {
                    _logger.LogInformation("[ValidateAsync] Salida: IA deshabilitada para el negocio.");
                    return AIChatValidationResult.EarlyExit("");
                }

                var normalizedMessage = request.UserMessage?.Trim().ToLower() ?? "";
                _logger.LogDebug("[ValidateAsync] NormalizedMessage len: {Len}", normalizedMessage.Length);

                if (normalizedMessage.Length < 20 &&
                    CourtesyWords.Any(w => normalizedMessage.Contains(w)))
                {
                    _logger.LogInformation("[ValidateAsync] Salida: respuesta de cortesía.");
                    return AIChatValidationResult.EarlyExit(CourtesyResponse, normalizedMessage);
                }

                _logger.LogDebug("[ValidateAsync] Continue.");
                return AIChatValidationResult.Continue(normalizedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ValidateAsync] Excepción. BusinessId: {BusinessId}", request?.BusinessId);
                throw;
            }
        }
    }
}
