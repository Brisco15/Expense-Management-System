using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace ExpenseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExpenses()
        {
            var expenses = await _expenseService.GetAllExpensesAsync();
            if (expenses == null)
            {
                return NotFound("No expenses found.");
            }
            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
            {
                return NotFound($"Expense with ID {id} not found.");
            }
            return Ok(expense);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserExpenses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expenses = await _expenseService.GetUserExpensesAsync(userId!);
            if (expenses == null)
            {
                return NotFound("No expenses found for the user.");
            }
            return Ok(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseDto createExpenseDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expense = await _expenseService.CreateExpenseAsync(createExpenseDto, userId);

            if (expense == null)
            {
                return NotFound("Failed to create expense.");
            }

            return Ok(expense);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            if (!result)
            {
                return NotFound($"Expense with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
