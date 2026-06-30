using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ExpenseManagement.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;

            var recentExpenses = await _context.Expenses
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .Select(e => new RecentExpenseItem
                {
                    Id = e.Id,
                    Title = e.Title,
                    Amount = e.Amount,
                    Category = e.Category!.Name,
                    Status = e.Status.ToString(),
                    ExpenseDate = e.ExpenseDate,
                    CreatedBy = e.User!.FullName
                })
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalExpenses = await _context.Expenses.SumAsync(e => e.Amount),

                TotalExpensesThisMonth = await _context.Expenses
                    .Where(e => e.ExpenseDate.Year == now.Year && e.ExpenseDate.Month == now.Month)
                    .SumAsync(e => e.Amount),

                TotalExpensesThisYear = await _context.Expenses
                    .Where(e => e.ExpenseDate.Year == now.Year)
                    .SumAsync(e => e.Amount),

                TotalExpenseCount = await _context.Expenses.CountAsync(),

                ApprovedExpenses = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Approved)
                    .SumAsync(e => e.Amount),

                PendingExpenses = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Pending)
                    .SumAsync(e => e.Amount),

                RejectedExpenses = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Rejected)
                    .SumAsync(e => e.Amount),

                RejectedExpenseCount = await _context.Expenses
                    .Where(e => e.Status == ExpenseStatus.Rejected)
                    .CountAsync(),

                RecentExpenses = recentExpenses
            };
        }

        public async Task <List<MonthlyExpenseDto>> GetMonthlyExpensesAsync()
        {
            return await _context.Expenses.GroupBy(e => new
                {
                     e.ExpenseDate.Year,
                     e.ExpenseDate.Month
                })
                .Select(g => new MonthlyExpenseDto
                {
                    Month = g.Key.Year + "-" + g.Key.Month,
                    TotalAmount = g.Sum(e => e.Amount)
                }).ToListAsync();
        }

        public async Task<List<CategoryExpenseDto>> GetCategoryExpensesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryExpenseDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    MonthlyBudget = c.MonthlyBudget,
                    YearlyBudget = c.YearlyBudget,
                    TotalAmount = c.Expenses.Sum(e => e.Amount),
                    TotalExpenses = c.Expenses.Count()
                })
                .ToListAsync();
        }
    }
}
