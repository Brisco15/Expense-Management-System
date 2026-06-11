using ExpenseManagement.Application.DTOs;

namespace ExpenseManagement.Application.Interfaces
{
    public interface IBudgetService
    {
        Task<BudgetOverviewDto?> GetBudgetOverviewForUserAsync(int userId);
        Task<List<CategoryBudgetDto>> GetAllCategoryBudgetsAsync();
        Task<CategoryBudgetDto?> GetCategoryBudgetAsync(int categoryId, int userId);
        Task<CategoryBudgetDto?> UpdateCategoryBudgetAsync(int categoryId, UpdateCategoryBudgetDto updateCategoryBudgetDto);
        Task<List<CategoryBudgetDto>> GetBudgetAlertsAsync();
    }
}
