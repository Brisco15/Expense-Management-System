using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagement.Infrastructure.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;

        public BudgetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryBudgetDto>> GetAllCategoryBudgetsAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Load active categories
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            // Aggregate expenses per category in a single DB query
            var aggregates = await _context.Expenses
                .Where(e => e.Status != ExpenseStatus.Rejected)
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    MonthlyExpense = g.Sum(e => e.ExpenseDate >= monthStart ? e.Amount : 0m),
                    YearlyExpense = g.Sum(e => e.ExpenseDate >= yearStart ? e.Amount : 0m),
                    PendingCount = g.Count(e => e.Status == ExpenseStatus.Pending),
                    ApprovedCount = g.Count(e => e.Status == ExpenseStatus.Approved),
                    PendingAmount = g.Sum(e => e.Status == ExpenseStatus.Pending ? e.Amount : 0m),
                    ApprovedAmount = g.Sum(e => e.Status == ExpenseStatus.Approved ? e.Amount : 0m)
                })
                .ToListAsync();

            var aggDict = aggregates.ToDictionary(a => a.CategoryId);

            var result = categories.Select(cat =>
            {
                aggDict.TryGetValue(cat.Id, out var a);

                var monthlyExpense = a?.MonthlyExpense ?? 0m;
                var yearlyExpense = a?.YearlyExpense ?? 0m;
                var monthlyRemaining = cat.MonthlyBudget.HasValue ? (cat.MonthlyBudget.Value - monthlyExpense) : (decimal?)null;
                var yearlyRemaining = cat.YearlyBudget.HasValue ? (cat.YearlyBudget.Value - yearlyExpense) : (decimal?)null;
                var isMonthlyOver = cat.MonthlyBudget.HasValue ? monthlyExpense > cat.MonthlyBudget.Value : (bool?)null;
                var isYearlyOver = cat.YearlyBudget.HasValue ? yearlyExpense > cat.YearlyBudget.Value : (bool?)null;

                return new CategoryBudgetDto
                {
                    CategoryId = cat.Id,
                    CategoryName = cat.Name,
                    MonthlyBudget = cat.MonthlyBudget,
                    MonthlyExpense = monthlyExpense,
                    RemainingMonthlyBudget = monthlyRemaining,
                    YearlyBudget = cat.YearlyBudget,
                    YearlyExpense = yearlyExpense,
                    RemainingYearlyBudget = yearlyRemaining,
                    IsMonthlyOverBudget = isMonthlyOver,
                    IsYearlyOverBudget = isYearlyOver,
                    PendingExpensesCount = a?.PendingCount ?? 0,
                    ApprovedExpensesCount = a?.ApprovedCount ?? 0,
                    PendingExpensesAmount = a?.PendingAmount ?? 0m,
                    ApprovedExpensesAmount = a?.ApprovedAmount ?? 0m
                };
            }).ToList();

            return result;
        }

        public async Task<CategoryBudgetDto?> GetCategoryBudgetAsync(int categoryId, int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

            if (category == null) return null;

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Single aggregation query instead of 6 separate round-trips
            var agg = await _context.Expenses
                .Where(e => e.CategoryId == categoryId && e.UserId == userId && e.Status != ExpenseStatus.Rejected)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    MonthlyExpense  = g.Sum(e => e.ExpenseDate >= monthStart ? e.Amount : 0m),
                    YearlyExpense   = g.Sum(e => e.ExpenseDate >= yearStart  ? e.Amount : 0m),
                    PendingCount    = g.Count(e => e.Status == ExpenseStatus.Pending),
                    ApprovedCount   = g.Count(e => e.Status == ExpenseStatus.Approved),
                    PendingAmount   = g.Sum(e => e.Status == ExpenseStatus.Pending  ? e.Amount : 0m),
                    ApprovedAmount  = g.Sum(e => e.Status == ExpenseStatus.Approved ? e.Amount : 0m)
                })
                .FirstOrDefaultAsync();

            var monthlyExpense = agg?.MonthlyExpense ?? 0m;
            var yearlyExpense  = agg?.YearlyExpense  ?? 0m;

            return new CategoryBudgetDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                MonthlyBudget = category.MonthlyBudget,
                MonthlyExpense = monthlyExpense,
                RemainingMonthlyBudget = category.MonthlyBudget.HasValue ? (category.MonthlyBudget.Value - monthlyExpense) : (decimal?)null,
                YearlyBudget = category.YearlyBudget,
                YearlyExpense = yearlyExpense,
                RemainingYearlyBudget = category.YearlyBudget.HasValue ? (category.YearlyBudget.Value - yearlyExpense) : (decimal?)null,
                IsMonthlyOverBudget = category.MonthlyBudget.HasValue ? monthlyExpense > category.MonthlyBudget.Value : (bool?)null,
                IsYearlyOverBudget  = category.YearlyBudget.HasValue  ? yearlyExpense  > category.YearlyBudget.Value  : (bool?)null,
                PendingExpensesCount   = agg?.PendingCount  ?? 0,
                ApprovedExpensesCount  = agg?.ApprovedCount ?? 0,
                PendingExpensesAmount  = agg?.PendingAmount  ?? 0m,
                ApprovedExpensesAmount = agg?.ApprovedAmount ?? 0m
            };
        }

        public async Task<CategoryBudgetDto?> UpdateCategoryBudgetAsync(int categoryId, UpdateCategoryBudgetDto updateCategoryBudgetDto)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null) return null;

            if (updateCategoryBudgetDto.MonthlyBudget.HasValue)
                category.MonthlyBudget = updateCategoryBudgetDto.MonthlyBudget.Value;

            if (updateCategoryBudgetDto.YearlyBudget.HasValue)
                category.YearlyBudget = updateCategoryBudgetDto.YearlyBudget.Value;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            // return updated snapshot — single aggregation query
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            var agg = await _context.Expenses
                .Where(e => e.CategoryId == categoryId && e.Status != ExpenseStatus.Rejected)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    MonthlyExpense = g.Sum(e => e.ExpenseDate >= monthStart ? e.Amount : 0m),
                    YearlyExpense  = g.Sum(e => e.ExpenseDate >= yearStart  ? e.Amount : 0m),
                    PendingCount   = g.Count(e => e.Status == ExpenseStatus.Pending),
                    ApprovedCount  = g.Count(e => e.Status == ExpenseStatus.Approved),
                    PendingAmount  = g.Sum(e => e.Status == ExpenseStatus.Pending  ? e.Amount : 0m),
                    ApprovedAmount = g.Sum(e => e.Status == ExpenseStatus.Approved ? e.Amount : 0m)
                })
                .FirstOrDefaultAsync();

            var monthlyExpense = agg?.MonthlyExpense ?? 0m;
            var yearlyExpense  = agg?.YearlyExpense  ?? 0m;

            return new CategoryBudgetDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                MonthlyBudget = category.MonthlyBudget,
                MonthlyExpense = monthlyExpense,
                RemainingMonthlyBudget = category.MonthlyBudget.HasValue ? (category.MonthlyBudget.Value - monthlyExpense) : (decimal?)null,
                YearlyBudget = category.YearlyBudget,
                YearlyExpense = yearlyExpense,
                RemainingYearlyBudget = category.YearlyBudget.HasValue ? (category.YearlyBudget.Value - yearlyExpense) : (decimal?)null,
                IsMonthlyOverBudget = category.MonthlyBudget.HasValue ? monthlyExpense > category.MonthlyBudget.Value : (bool?)null,
                IsYearlyOverBudget  = category.YearlyBudget.HasValue  ? yearlyExpense  > category.YearlyBudget.Value  : (bool?)null,
                PendingExpensesCount   = agg?.PendingCount  ?? 0,
                ApprovedExpensesCount  = agg?.ApprovedCount ?? 0,
                PendingExpensesAmount  = agg?.PendingAmount  ?? 0m,
                ApprovedExpensesAmount = agg?.ApprovedAmount ?? 0m
            };
        }

        public async Task<List<CategoryBudgetDto>> GetBudgetAlertsAsync()
        {
            var all = await GetAllCategoryBudgetsAsync();
            return all.Where(c => (c.IsMonthlyOverBudget == true) || (c.IsYearlyOverBudget == true)).ToList();
        }

        public async Task<BudgetOverviewDto?> GetBudgetOverviewForUserAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            var userExpenses = _context.Expenses
                .Where(e => e.UserId == userId && e.Status != ExpenseStatus.Rejected);

            var totalThisMonth = await userExpenses.Where(e => e.ExpenseDate >= monthStart).SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var totalThisYear = await userExpenses.Where(e => e.ExpenseDate >= yearStart).SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var pendingCount  = await userExpenses.CountAsync(e => e.Status == ExpenseStatus.Pending);
            var pendingAmount = await userExpenses
                .Where(e => e.Status == ExpenseStatus.Pending)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            // per-category budgets for this user (only categories that are active)
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            var expensesByCategory = await userExpenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    MonthlyExpense = g.Sum(e => e.ExpenseDate >= monthStart ? e.Amount : 0m),
                    YearlyExpense = g.Sum(e => e.ExpenseDate >= yearStart ? e.Amount : 0m)
                })
                .ToListAsync();

            var expenseDict = expensesByCategory.ToDictionary(x => x.CategoryId);

            var categoryBudgets = categories.Select(cat =>
            {
                expenseDict.TryGetValue(cat.Id, out var a);
                var monthlyExpense = a?.MonthlyExpense ?? 0m;
                var yearlyExpense = a?.YearlyExpense ?? 0m;
                return new CategoryBudgetDto
                {
                    CategoryId = cat.Id,
                    CategoryName = cat.Name,
                    MonthlyBudget = cat.MonthlyBudget,
                    MonthlyExpense = monthlyExpense,
                    RemainingMonthlyBudget = cat.MonthlyBudget.HasValue ? (cat.MonthlyBudget.Value - monthlyExpense) : (decimal?)null,
                    YearlyBudget = cat.YearlyBudget,
                    YearlyExpense = yearlyExpense,
                    RemainingYearlyBudget = cat.YearlyBudget.HasValue ? (cat.YearlyBudget.Value - yearlyExpense) : (decimal?)null,
                    IsMonthlyOverBudget = cat.MonthlyBudget.HasValue ? monthlyExpense > cat.MonthlyBudget.Value : (bool?)null,
                    IsYearlyOverBudget = cat.YearlyBudget.HasValue ? yearlyExpense > cat.YearlyBudget.Value : (bool?)null,
                    PendingExpensesCount = 0,
                    ApprovedExpensesCount = 0,
                    PendingExpensesAmount = 0,
                    ApprovedExpensesAmount = 0
                };
            }).ToList();

            return new BudgetOverviewDto
            {
                TotalExpensesThisMonth = totalThisMonth,
                TotalExpensesThisYear = totalThisYear,
                PendingExpensesCount = pendingCount,
                PendingExpensesAmount = pendingAmount,
                CategoryBudgets = categoryBudgets
            };
        }
    }
}
