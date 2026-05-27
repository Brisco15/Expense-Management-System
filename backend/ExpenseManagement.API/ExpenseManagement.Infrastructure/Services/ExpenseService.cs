using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ExpenseManagement.Infrastructure.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;

        public ExpenseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExpenseDto>> GetAllExpensesAsync()
        {
            var expenses = await _context.Expenses
                            .Include(e => e.User)
                            .Include(e => e.Category)
                            .ToListAsync();

            return expenses.Select(e => new ExpenseDto
            {
                Id = e.Id,
                Title = e.Title,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Status = e.Status.ToString(),
                Description = e.Description,
                Category = e.Category!.Name,
                User = e.User!.FullName
            }).ToList();
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            var expense = await _context.Expenses
                            .Include(e => e.User)
                            .Include(e => e.Category)
                            .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null) { return null; }

            return new ExpenseDto
            {
                Id = expense.Id,
                Title = expense.Title,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Status = expense.Status.ToString(),
                Description = expense.Description,
                Category = expense.Category!.Name,
                User = expense.User!.FullName
            };
        }

        public async Task<ExpenseDto> CreateExpenseAsync(
            CreateExpenseDto createExpenseDto,
            int userId
            )
        {
            var expense = new Expense
            {
                Title = createExpenseDto.Title,
                Amount = createExpenseDto.Amount,
                ExpenseDate = createExpenseDto.ExpenseDate,
                Description = createExpenseDto.Description,
                CategoryId = createExpenseDto.CategoryId,
                UserId = userId,
                Status = ExpenseStatus.Pending
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return await GetExpenseByIdAsync(expense.Id)
                ?? throw new Exception("Failed to create expense.");
        }

        public async Task<List<ExpenseDto>> GetUserExpensesAsync(string userId)
        {
            var expenses = await _context.Expenses
                            .Where(e => e.UserId.ToString() == userId)
                            .Include(e => e.Category)
                            .ToListAsync();
            return expenses.Select(e => new ExpenseDto
            {
                Id = e.Id,
                Title = e.Title,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Status = e.Status.ToString(),
                Description = e.Description,
                Category = e.Category!.Name
            }).ToList();
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if(expense == null) { return false; }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
