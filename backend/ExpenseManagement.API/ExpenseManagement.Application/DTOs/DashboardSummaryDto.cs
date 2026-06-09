

namespace ExpenseManagement.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalExpenses { get; set; } = decimal.Zero;

        public decimal TotalExpensesThisMonth { get; set; } = decimal.Zero;
        public decimal TotalExpensesThisYear { get; set; } = decimal.Zero;
        public int TotalExpenseCount { get; set; } = 0;
        public decimal ApprovedExpenses { get; set;} = decimal.Zero;
        public decimal PendingExpenses { get; set;} = decimal.Zero;

        //public List<CategoryBudgetDto> CategoryBudgets { get; set; } = new List<CategoryBudgetDto>();
        //public List<ExpenseDto> RecentExpenses { get; set; } = new List<ExpenseDto>();

    }
}
