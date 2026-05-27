using ExpenseManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync();

            if (summary == null)
            {
                return NotFound("Dashboard summary not found.");
            }

            return Ok(summary);
        }

        [HttpGet("monthly-expenses")]
        public async Task<IActionResult> GetMonthlyExpenses()
        {
            var monthlyExpenses = await _dashboardService.GetMonthlyExpensesAsync();
            if (monthlyExpenses == null)
            {
                return NotFound("Monthly expenses not found.");
            }
            return Ok(monthlyExpenses);

        }

        [HttpGet("category-expenses")]
        public async Task<IActionResult> GetCategoryExpenses()
        {
            var categoryExpenses = await _dashboardService.GetCategoryExpensesAsync();
            if (categoryExpenses == null)
            {
                return NotFound("Category expenses not found.");
            }
            return Ok(categoryExpenses);
        }
    }
}
