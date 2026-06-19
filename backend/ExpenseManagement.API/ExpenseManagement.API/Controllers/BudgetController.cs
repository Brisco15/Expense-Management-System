using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
    

namespace ExpenseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]

    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet("categories")]
        [Authorize(Policy="AdminOrManager")]
        public async Task<IActionResult> GetAllCategoryBudget()
        {
            var budgets = await _budgetService.GetAllCategoryBudgetsAsync();
            if(budgets == null || !budgets.Any())
            {
                return Ok( new List<CategoryBudgetDto>());
            }
            return Ok(budgets);

        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetBudgetOverview()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var budgetoverview = await _budgetService.GetBudgetOverviewForUserAsync(userId);
            if(budgetoverview == null)
            {
                return NotFound("No budget overview available.");
            }
            return Ok(budgetoverview);
        }

        [HttpGet("alerts")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetBudgetAlerts()
        {
            var alerts = await _budgetService.GetBudgetAlertsAsync();
            if(alerts == null || !alerts.Any())
            {
                return Ok( new List<CategoryBudgetDto>());
            }
            return Ok(alerts);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetBudgetCategoryForUser( int categoryId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var budget = await _budgetService.GetCategoryBudgetAsync(categoryId, userId);
            if(budget == null)
            {
                return NotFound($" No budget found for category {categoryId}.");
            }
            return Ok(budget);
        }

        [HttpPut("categories/{categoryId}")]
        [Authorize(Policy ="AdminOnly")]
        public async Task<IActionResult> UpdateCategorybudget(int categoryId,[FromBody] UpdateCategoryBudgetDto updateCategoryBudgetDto)
        {
           
            if (updateCategoryBudgetDto.MonthlyBudget.HasValue && updateCategoryBudgetDto.YearlyBudget.HasValue)
            {
                return BadRequest("At least one budget value must be provided");
            }

            var result = await _budgetService.UpdateCategoryBudgetAsync(categoryId, updateCategoryBudgetDto);
            if (result == null)
            {
                return NotFound($"Category {categoryId} not found ");
            }

            return Ok(result);

        }
    }
}
