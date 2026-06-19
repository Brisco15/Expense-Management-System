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
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetAllExpenses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false

            )
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 10)
            {
                return BadRequest("PageNumber must be >= 1, PageSize must be 1-10.");
            }

            var expenses = await _expenseService.GetAllExpensesAsync(pageNumber, pageSize, includeInactive);
            if (expenses == null || !expenses.Items.Any())
            {
                return Ok(new PagedResult<ExpenseDto>
                {
                    Items = new List<ExpenseDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0
                });
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

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            if(expense.CreatedByUserId != userId && !isAdminOrManager)
            {
                return Forbid();
            }

            return Ok(expense);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserExpenses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expenses = await _expenseService.GetUserExpensesAsync(userId);
            if (expenses == null)
            {
                return NotFound("No expenses found for the user.");
            }
            return Ok(expenses);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetExpensesByCategory(int categoryId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expenses = await _expenseService.GetExpensesByCategoryAsync(categoryId, userId);
            if (expenses == null || !expenses.Any())
            {
                return NotFound($"No expenses found for category ID {categoryId}.");
            }
            return Ok(expenses);
        }

        [HttpGet("budget")]
        public async Task<IActionResult> GetBudgetOverview()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var budgetOverview = await _expenseService.GetBudgetOverviewAsync(userId);
            if (budgetOverview == null)
            {
                return NotFound("No budget overview found for the user.");
            }
            return Ok(budgetOverview);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto createExpenseDto)
        {
            if (!ModelState.IsValid) 
            { 
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expense = await _expenseService.CreateExpenseAsync(createExpenseDto, userId);

            if (expense == null)
            {
                return BadRequest("Failed to create expense.");
            }

            return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, expense);
        }

        [HttpPost("{id}/receipt")]
        public async Task<IActionResult> UploadReceipt(int id, IFormFile receipt)
        {
            if (receipt == null || receipt.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(receipt.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and PDF files are allowed.");
            }
            // Validate file size  max 5MB
            if (receipt.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size exceeds the limit of 5MB.");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);



            var result = await _expenseService.UploadReceiptAsync(id, receipt.OpenReadStream(), receipt.FileName, userId);
            if (result == null)
            {
                return NotFound($"Expense with ID {id} not found or failed to upload receipt.");
            }
            return CreatedAtAction(nameof(GetExpenseById), new { id = id }, new { receiptPath = result });
        }

        [HttpPost("{id}/approve")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ApproveExpense(int id, [FromBody] ApproveExpenseDto approveExpenseDto)
        {
            if(id != approveExpenseDto.ExpenseId)
            {
                return BadRequest("Expense ID mismatch.");
            }
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expense = await _expenseService.ApproveExpenseAsync(approveExpenseDto, userId);
            if (expense == null)
            {
                return NotFound("Failed to approve/reject expense.");
            }
            return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, expense);
        }

        [HttpGet("pending")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetPendingExpenses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expenses = await _expenseService.GetPendingExpensesAsync(userId);
            if (expenses == null || !expenses.Any())
            {
                return NotFound("No pending expenses found.");
            }
            return Ok(expenses);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto updateExpenseDto)
        {
            if(id != updateExpenseDto.Id)
            {
                return BadRequest("Expense ID mismatch.");
            }
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var expense = await _expenseService.UpdateExpenseAsync(updateExpenseDto, userId);

            if (expense == null)
            {
                return NotFound("Failed to update expense.");
            }

            return Ok(expense);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            if (!result)
            {
                return NotFound($"Expense with ID {id} not found.");
            }
            return NoContent();
        }

        [HttpDelete("{id}/receipt")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _expenseService.DeleteReceiptAsync(id, userId);
            if (!result)
            {
                return NotFound($"Receipt for expense with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
