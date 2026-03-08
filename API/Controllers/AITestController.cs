using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AITestController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AITestController> _logger;

        public AITestController(IAIService aiService, ILogger<AITestController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AIRequest request)
        {
            _logger.LogInformation(
                "[AI Ask] Request. BusinessId: {BusinessId}, Phone: {Phone}, Message length: {Len}, Channel: {Channel}",
                request.BusinessId, request.PhoneNumber ?? "(null)", request.UserMessage?.Length ?? 0, request.Channel ?? "(null)");

            var response = await _aiService.AskAsync(request);

            return Ok(response);
        }
    }
}
