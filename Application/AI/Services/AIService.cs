using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Cache;
using MiNegocioCR.Api.Application.AI.Guardrails;
using MiNegocioCR.Api.Application.AI.Intent;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Limits;
using MiNegocioCR.Api.Application.AI.Memory;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.AI.Prompts;
using MiNegocioCR.Api.Application.AI.Routing;
using MiNegocioCR.Api.Application.AI.Sales;
using MiNegocioCR.Api.Application.AI.State;
using MiNegocioCR.Api.Application.AI.Tools;
using MiNegocioCR.Api.Application.AI.Upsell;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MiNegocioCR.Api.Application.AI.Services
{
    public class AIService : IAIService
    {
        private readonly IPromptBuilder _promptBuilder;
        private readonly IDomainFilter _domainFilter;
        private readonly IAIClient _aiClient;
        private readonly IEnumerable<IAITool> _tools;
        private readonly IModelRouter _modelRouter;
        private readonly IResponseCache _cache;
        private readonly ITokenLimiter _tokenLimiter;
        private readonly IConversationMemoryService _memory;
        private readonly IConversationStateService _state;
        private readonly IIntentClassifier _intentClassifier;
        private readonly IToolSelector _toolSelector;
        private readonly AppDbContext _context;
        private readonly IAITokenBudgetService _tokenBudget;
        private readonly IUpsellService _upsellService;
        private readonly IAIChatRequestValidator _requestValidator;
        private readonly ISalesConversationHandler _salesConversationHandler;
        private readonly ILogger<AIService> _logger;

        public AIService(
            IPromptBuilder promptBuilder,
            IDomainFilter domainFilter,
            IAIClient aiClient,
            IEnumerable<IAITool> tools,
            IModelRouter modelRouter,
            IResponseCache cache,
            ITokenLimiter tokenLimiter,
            IConversationMemoryService memory,
            IConversationStateService state,
            IIntentClassifier intentClassifier,
            IToolSelector toolSelector,
            IAITokenBudgetService tokenBudget,
            AppDbContext context,
            IUpsellService upsellService,
            IAIChatRequestValidator requestValidator,
            ISalesConversationHandler salesConversationHandler,
            ILogger<AIService> logger)
        {
            _promptBuilder = promptBuilder;
            _domainFilter = domainFilter;
            _aiClient = aiClient;
            _tools = tools;
            _modelRouter = modelRouter;
            _cache = cache;
            _tokenLimiter = tokenLimiter;
            _memory = memory;
            _state = state;
            _intentClassifier = intentClassifier;
            _toolSelector = toolSelector;
            _context = context;
            _tokenBudget = tokenBudget;
            _upsellService = upsellService;
            _requestValidator = requestValidator;
            _salesConversationHandler = salesConversationHandler;
            _logger = logger;
        }

        public async Task<string> AskAsync(AIRequest request)
        {
            try
            {
                _logger.LogInformation("[AskAsync] Inicio. BusinessId: {BusinessId}, Phone: {Phone}, UserMessageLen: {Len}",
                    request.BusinessId, request.PhoneNumber ?? "(null)", request.UserMessage?.Length ?? 0);

                var validation = await _requestValidator.ValidateAsync(request);
            _logger.LogDebug("[AskAsync] Validación. CanContinue: {CanContinue}, EarlyResponse: {EarlyResponse}",
                validation.CanContinue, validation.EarlyResponse != null ? "(set)" : "(null)");
            if (!validation.CanContinue)
                return validation.EarlyResponse ?? "";

            var normalizedMessage = validation.NormalizedMessage;

            await _memory.SaveMessageAsync(
                request.BusinessId,
                request.PhoneNumber ?? "",
                "user",
                request.UserMessage);

            var conversationState = await _state.GetAsync(
                request.BusinessId,
                request.PhoneNumber);
            _logger.LogDebug("[AskAsync] ConversationState: {HasState}, DomainFilter allowed: {Allowed}",
                conversationState != null, _domainFilter.IsAllowed(request.UserMessage));

            if (conversationState == null && !_domainFilter.IsAllowed(request.UserMessage))
            {
                _logger.LogInformation("[AskAsync] Salida por dominio no permitido.");
                return "Lo siento, solo puedo ayudarte con productos, servicios o reparaciones del negocio.";
            }

            var salesResponse = await _salesConversationHandler.HandleAsync(
                request.BusinessId,
                request.PhoneNumber ?? "",
                normalizedMessage,
                conversationState);

            if (salesResponse != null)
            {
                _logger.LogInformation("[AskAsync] Salida por sales response (longitud {Len}).", salesResponse.Length);
                return salesResponse;
            }

            // Cache
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var cacheKey = $"{request.BusinessId}:{request.PhoneNumber}:{today}:{normalizedMessage}";

            var cached = await _cache.GetAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("[AskAsync] Salida por cache hit.");
                return cached;
            }

            // Seleccionar y ejecutar tool
            var intent = _intentClassifier.Classify(normalizedMessage);
            var selectedTool = _toolSelector.Select(intent);
            _logger.LogInformation("[AskAsync] Intent: {Intent}, Tool: {Tool}", intent, selectedTool?.Name ?? "(null)");

            ToolResult toolData;
            if (selectedTool.Name == "repair_order_search")
            {
                toolData = await selectedTool.ExecuteAsync(
                    request.BusinessId,
                    request.PhoneNumber ?? "");
            }
            else
            {
                toolData = await selectedTool.ExecuteAsync(
                    request.BusinessId,
                    request.UserMessage);
            }

            _logger.LogDebug("[AskAsync] Tool ejecutado. ProductId: {ProductId}, MessageLen: {Len}",
                toolData?.ProductId, toolData?.Message?.Length ?? 0);

            if (selectedTool.Name == "repair_service_search")
            {
                _logger.LogInformation("[AskAsync] Salida por repair_service_search.");
                return toolData.Message;
            }

            if (toolData.ProductId == null)
            {
                var fallback = _tools.FirstOrDefault(t => t.Name == "inventory_search");
                if (fallback != null)
                {
                    var fallbackResult = await fallback.ExecuteAsync(
                        request.BusinessId,
                        request.UserMessage);

                    if (fallbackResult.ProductId != null)
                        toolData = fallbackResult;
                }
                _logger.LogDebug("[AskAsync] Fallback inventory. ProductId después: {ProductId}", toolData?.ProductId);
            }

            if (toolData.ProductId != null)
            {
                _logger.LogDebug("[AskAsync] Buscando variante y upsell para ProductId: {ProductId}", toolData.ProductId);
                var variant = await _context.CatalogVariants
                    .AsNoTracking()
                    .Include(v => v.CatalogItem)
                    .FirstOrDefaultAsync(v => v.Id == toolData.ProductId.Value);

                if (variant != null)
                {
                    _logger.LogDebug("[AskAsync] Variante encontrada, aplicando upsell.");
                    var upsells = await _upsellService.GetUpsell(
                        request.BusinessId,
                        variant.CatalogItemId);

                    if (upsells.Any())
                    {
                        var suggestion = upsells.First();
                        toolData.Message +=
                            $"\n\nTambién podrías agregar {suggestion.Name} por ₡{suggestion.BasePrice:N0}.";
                    }
                    else
                    {
                        var fallbackUpsell = await _upsellService.GetFallbackUpsell(request.BusinessId);
                        if (fallbackUpsell.Any())
                        {
                            toolData.Message += "\n\nTambién podrías agregar:\n";
                            foreach (var item in fallbackUpsell.Take(2))
                                toolData.Message += $"• {item.Name} ₡{item.BasePrice:N0}\n";
                        }
                    }
                }

                await _state.SaveAsync(new ConversationState
                {
                    Id = Guid.NewGuid(),
                    BusinessId = request.BusinessId,
                    PhoneNumber = request.PhoneNumber ?? "",
                    ProductId = toolData.ProductId,
                    ProductName = variant?.CatalogItem?.Name ?? toolData.ProductName,
                    Step = "awaiting_confirmation",
                    UpdatedAt = DateTime.UtcNow
                });
                _logger.LogDebug("[AskAsync] ConversationState guardado.");
            }

            _logger.LogDebug("[AskAsync] Construyendo prompt y llamando a IA.");
            var history = await _memory.GetConversationContextAsync(
                request.BusinessId,
                request.PhoneNumber ?? "",
                10);

            var prompt = $"""
            Conversación reciente:
            {history}

            {_promptBuilder.BuildPrompt("Mi Negocio", request.UserMessage)}

            Data:
            {toolData.Message}
            """;

            var model = _modelRouter.SelectModel(prompt);
            var maxTokens = _tokenLimiter.GetMaxTokens(prompt);
            var estimatedTokens = prompt.Length / 4;

            var allowed = await _tokenBudget.CanUseAsync(
                request.BusinessId,
                estimatedTokens);
            _logger.LogDebug("[AskAsync] Token budget allowed: {Allowed}, model: {Model}, maxTokens: {MaxTokens}", allowed, model, maxTokens);

            if (!allowed)
            {
                _logger.LogWarning("[AskAsync] Salida por token budget agotado.");
                return "La IA no está disponible en este momento.";
            }

            var response = await _aiClient.AskAsync(prompt, model, maxTokens);
            _logger.LogInformation("[AskAsync] Respuesta IA recibida (longitud {Len}).", response?.Length ?? 0);

            await _memory.SaveMessageAsync(
                request.BusinessId,
                request.PhoneNumber ?? "",
                "assistant",
                response);

            await _cache.SetAsync(cacheKey, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AskAsync] Excepción. BusinessId: {BusinessId}, Phone: {Phone}", request.BusinessId, request.PhoneNumber ?? "(null)");
                throw;
            }
        }
    }
}
