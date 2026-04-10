using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICreateCategoryUseCase _createCategory;
        private readonly IGetCategoriesByBusinessUseCase _getCategoriesByBusiness;

        public CategoriesController(
            ICreateCategoryUseCase createCategory,
            IGetCategoriesByBusinessUseCase getCategoriesByBusiness)
        {
            _createCategory = createCategory;
            _getCategoriesByBusiness = getCategoriesByBusiness;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var id = await _createCategory.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpGet("{businessId:guid}")]
        public async Task<IActionResult> GetByBusiness([FromRoute] Guid businessId)
        {
            var categories = await _getCategoriesByBusiness.ExecuteAsync(businessId);
            return Ok(categories);
        }
    }
}
