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

        public AITestController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AIRequest request)
        {
            var response = await _aiService.AskAsync(request);

            return Ok(response);
        }
    }
}
