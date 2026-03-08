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
            ISalesConversationHandler salesConversationHandler)
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
        }

        public async Task<string> AskAsync(AIRequest request)
        {
            var validation = await _requestValidator.ValidateAsync(request);
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

            if (conversationState == null && !_domainFilter.IsAllowed(request.UserMessage))
            {
                return "Lo siento, solo puedo ayudarte con productos, servicios o reparaciones del negocio.";
            }

            var salesResponse = await _salesConversationHandler.HandleAsync(
                request.BusinessId,
                request.PhoneNumber ?? "",
                normalizedMessage,
                conversationState);

            if (salesResponse != null)
                return salesResponse;

            // Cache
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var cacheKey = $"{request.BusinessId}:{request.PhoneNumber}:{today}:{normalizedMessage}";

            var cached = await _cache.GetAsync(cacheKey);
            if (cached != null)
                return cached;

            // Seleccionar y ejecutar tool
            var intent = _intentClassifier.Classify(normalizedMessage);
            var selectedTool = _toolSelector.Select(intent);

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

            if (selectedTool.Name == "repair_service_search")
            {
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
            }

            if (toolData.ProductId != null)
            {
                var variant = await _context.CatalogVariants
                    .AsNoTracking()
                    .Include(v => v.CatalogItem)
                    .FirstOrDefaultAsync(v => v.Id == toolData.ProductId.Value);

                if (variant != null)
                {
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
            }

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

            if (!allowed)
                return "La IA no está disponible en este momento.";

            var response = await _aiClient.AskAsync(prompt, model, maxTokens);

            await _memory.SaveMessageAsync(
                request.BusinessId,
                request.PhoneNumber ?? "",
                "assistant",
                response);

            await _cache.SetAsync(cacheKey, response);

            return response;
        }
    }
}
