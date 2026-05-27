using ExpenseManagement.Application.DTOs;

namespace ExpenseManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task <DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<List<MonthlyExpenseDto>> GetMonthlyExpensesAsync();
        Task<List<CategoryExpenseDto>> GetCategoryExpensesAsync();
    }
}
