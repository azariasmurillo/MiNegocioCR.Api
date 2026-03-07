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
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.Services
{
    public class AIService : IAIService
    {
        private readonly SalesPromptBuilder _promptBuilder;
        private readonly DomainFilter _domainFilter;
        private readonly IAIClient _aiClient;
        private readonly IEnumerable<IAITool> _tools;
        private readonly IModelRouter _modelRouter;
        private readonly IResponseCache _cache;
        private readonly ITokenLimiter _tokenLimiter;
        private readonly IConversationMemoryService _memory;
        private readonly IConversationStateService _state;
        private readonly SaleService _saleService;
        private readonly IntentClassifier _intentClassifier;
        private readonly ToolSelector _toolSelector;
        private readonly AppDbContext _context;
        private readonly AITokenBudgetService _tokenBudget;

        public AIService(
            SalesPromptBuilder promptBuilder,
            DomainFilter domainFilter,
            IAIClient aiClient,
            IEnumerable<IAITool> tools,
            IModelRouter modelRouter,
            IResponseCache cache,
            ITokenLimiter tokenLimiter,
            IConversationMemoryService memory,
            IConversationStateService state,
            IntentClassifier intentClassifier,
            ToolSelector toolSelector,
            SaleService saleService,
            AITokenBudgetService tokenBudget,
            AppDbContext context)
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
            _saleService = saleService;
            _intentClassifier = intentClassifier;
            _toolSelector = toolSelector;
            _context = context;
            _tokenBudget = tokenBudget;
        }

        public async Task<string> AskAsync(AIRequest request)
        {
            var settings = await _context.BusinessSettings
                .FirstOrDefaultAsync(x => x.BusinessId == request.BusinessId);

            if (settings != null && !settings.EnableAIChat)
            {
                return "";
            }

            // 1️⃣ Guardrail
            if (!_domainFilter.IsAllowed(request.UserMessage))
            {
                return "Lo siento, solo puedo ayudarte con productos, servicios o reparaciones del negocio.";
            }

            var normalizedMessage = request.UserMessage.Trim().ToLower();

            // 2️⃣ Revisar estado de conversación
            var conversationState = await _state.GetAsync(
                request.BusinessId,
                request.PhoneNumber);

            if (conversationState != null &&
                conversationState.Step == "awaiting_confirmation")
            {
                if (normalizedMessage.Contains("si") || normalizedMessage.Contains("sí"))
                {
                    var result = await _saleService.CreateSaleAsync(
                        request.BusinessId,
                        conversationState.ProductId!.Value,
                        request.PhoneNumber);

                    await _state.ClearAsync(
                        request.BusinessId,
                        request.PhoneNumber);

                    return result;
                }

                if (normalizedMessage.Contains("no"))
                {
                    await _state.ClearAsync(
                        request.BusinessId,
                        request.PhoneNumber);

                    return "Compra cancelada.";
                }
            }

            // 3️⃣ Cache
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var cacheKey =
                $"{request.BusinessId}:{request.PhoneNumber}:{today}:{normalizedMessage}";

            var cached = await _cache.GetAsync(cacheKey);

            if (cached != null)
                return cached;

            // 4️⃣ Seleccionar Tool
            IAITool selectedTool;

            var intent = _intentClassifier.Classify(normalizedMessage);

            selectedTool = _toolSelector.Select(intent);

            // 5️⃣ Ejecutar tool
            var toolData = await selectedTool.ExecuteAsync(
                request.BusinessId,
                request.UserMessage);

            // 6️⃣ Construir prompt
            var prompt = $"""
{_promptBuilder.BuildPrompt("Mi Negocio", request.UserMessage)}

Data:
{toolData}
""";

            // 7️⃣ Modelo
            var model = _modelRouter.SelectModel(prompt);

            // 8️⃣ Tokens
            var maxTokens = _tokenLimiter.GetMaxTokens(prompt);

            var estimatedTokens = prompt.Length / 4;

            var allowed = await _tokenBudget.CanUseAsync(
                request.BusinessId,
                estimatedTokens);

            if (!allowed)
            {
                return "La IA no está disponible en este momento.";
            }

            // 9️⃣ Llamar IA
            var response = await _aiClient.AskAsync(prompt, model, maxTokens);

            // 🔟 Guardar cache
            await _cache.SetAsync(cacheKey, response);

            return response;
        }
    }
}