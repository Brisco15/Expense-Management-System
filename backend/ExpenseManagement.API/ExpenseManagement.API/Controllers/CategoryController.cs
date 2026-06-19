using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseManagement.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryService _categoryService;

        public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User ID claim not found in token");
                throw new UnauthorizedAccessException("User identity not found");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError("Invalid user ID format: {UserIdClaim}", userIdClaim);
                throw new UnauthorizedAccessException("Invalid user identifier format");
            }

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false
            )
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 10)
            {
                return BadRequest("PageNumber must be >= 1, PageSize must be 1-10.");
            }

            var categories = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize, includeInactive);
            if(categories == null || !categories.Items.Any())
            {
                return Ok(new PagedResult<CategoryExpenseDto> 
                {
                    Items = new List<CategoryExpenseDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0
                });
            }
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if(category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Policy ="AdminOnly")]

        public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} creating category '{CategoryName}'",
                    userId, createCategoryDto.CategoryName);

                var category = await _categoryService.CreateCategoryAsync(createCategoryDto, userId);

                _logger.LogInformation("Category {CategoryId} created successfully by user {UserId}",
                    category.CategoryId, userId);

                return CreatedAtAction(
                    nameof(GetCategoryById),
                    new { id = category.CategoryId },
                    category);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create category: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating category");
                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        
        [HttpPut("{id:int:min(1)}")]
        [Authorize(Policy = "AdminOnly")]
        
        public async Task<ActionResult<CategoryExpenseDto>> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != updateCategoryDto.CategoryId)
            {
                return BadRequest("Category ID mismatch");
            }

            try
            {
                _logger.LogInformation("Updating category {CategoryId}", id);

                var category = await _categoryService.UpdateCategoryAsync(updateCategoryDto, id);

                _logger.LogInformation("Category {CategoryId} updated successfully", id);

                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category {CategoryId} not found", id);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update category {CategoryId}: {Message}", id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for category {CategoryId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating category {CategoryId}", id);
                return StatusCode(500, "An error occurred while updating the category");
            }
        }

       
        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Policy = "AdminOnly")]
        
        public async Task<IActionResult> HardDelete(int id)
        {
            try
            {
                _logger.LogWarning("Hard deleting category {CategoryId}", id);

                var result = await _categoryService.DeleteCategoryAsync(id);

                if (!result)
                {
                    return NotFound($"Category with ID {id} not found.");
                }

                _logger.LogInformation("Category {CategoryId} deleted successfully", id);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete category {CategoryId}: {Message}", id, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting category {CategoryId}", id);
                return StatusCode(500, "An error occurred while deleting the category");
            }
        }

        
        [HttpPatch("{id:int:min(1)}/deactivate")]
        [Authorize(Policy = "AdminOnly")]
        
        public async Task<IActionResult> DeactivateCategory(int id)
        {
            try
            {
                _logger.LogInformation("Deactivating category {CategoryId}", id);

                var result = await _categoryService.SoftDeleteCategoryAsync(id);

                if (!result)
                {
                    return NotFound($"Category with ID {id} not found.");
                }

                _logger.LogInformation("Category {CategoryId} deactivated successfully", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deactivating category {CategoryId}", id);
                return StatusCode(500, "An error occurred while deactivating the category");
            }
        }

        
        
    }
}
