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
           
            return new DashboardSummaryDto
            {
                TotalExpenses =  await _context.Expenses.SumAsync(e => e.Amount),

                TotalExpenseCount = await _context.Expenses.CountAsync(),

                ApprovedExpenses = await _context.Expenses
                                    .Where(e => e.Status == ExpenseStatus.Approved)
                                    .SumAsync(e => e.Amount),

                PendingExpenses = await _context.Expenses
                                    .Where(e => e.Status == ExpenseStatus.Pending)
                                    .SumAsync(e => e.Amount)

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
            return await _context.Expenses
                .Include(e => e.Category)
                .GroupBy(e => e.Category!.Name)
                .Select(g => new CategoryExpenseDto
                {
                    CategoryName = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                }).ToListAsync();
        }
    }
}
