using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICreateCategoryUseCase _createCategory;
        private readonly IGetCategoriesByBusinessUseCase _getCategoriesByBusiness;
        private readonly IUpdateCategoryUseCase _updateCategory;
        private readonly IToggleCategoryStatusUseCase _toggleCategoryStatus;
        private readonly IDeleteCategoryUseCase _deleteCategory;

        public CategoriesController(
            ICreateCategoryUseCase createCategory,
            IGetCategoriesByBusinessUseCase getCategoriesByBusiness,
            IUpdateCategoryUseCase updateCategory,
            IToggleCategoryStatusUseCase toggleCategoryStatus,
            IDeleteCategoryUseCase deleteCategory)
        {
            _createCategory = createCategory;
            _getCategoriesByBusiness = getCategoriesByBusiness;
            _updateCategory = updateCategory;
            _toggleCategoryStatus = toggleCategoryStatus;
            _deleteCategory = deleteCategory;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var id = await _createCategory.ExecuteAsync(request);
            return Ok(id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _updateCategory.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpPatch("{id:guid}/toggle")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, [FromBody] ToggleCategoryStatusRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _toggleCategoryStatus.ExecuteAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, [FromQuery] Guid businessId)
        {
            await _deleteCategory.ExecuteAsync(id, businessId);
            return NoContent();
        }

        [HttpGet("{businessId:guid}")]
        public async Task<IActionResult> GetByBusiness([FromRoute] Guid businessId, [FromQuery] bool includeInactive = false)
        {
            var categories = await _getCategoriesByBusiness.ExecuteAsync(businessId, includeInactive);
            return Ok(categories);
        }
    }
}
