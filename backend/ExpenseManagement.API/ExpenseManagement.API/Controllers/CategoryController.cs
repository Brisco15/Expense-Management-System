using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExpenseManagement.Infrastructure.Services;


namespace ExpenseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryService _categoryService;

        public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            if(categories == null)
            {
                return NotFound("No categories found.");
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var category = await _categoryService.CreateCategoryAsync(createCategoryDto, userId);

            if(category == null)
            {
                return BadRequest("Failed to create category");
            }
            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]

        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if(id != updateCategoryDto.CategoryId)
            {
                return BadRequest("Category ID mismatch");
            }
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var category = await _categoryService.UpdateCategoryAsync(updateCategoryDto, userId);

            if(category == null)
            {
                return NotFound("Failed to update category");
            }
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy ="AdminOnly")]
        public async Task<IActionResult> HardDelete(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if(!result)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return NoContent();
        }

        [HttpDelete("{id}/Softdelete")]
        [Authorize(Policy ="Adminonly")]
        public async Task<IActionResult>SoftDelete(int id)
        {
            var result = await _categoryService.SoftDeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound($"Category with ID {id} not found.");
            }
            return NoContent();
        }

    }
}
