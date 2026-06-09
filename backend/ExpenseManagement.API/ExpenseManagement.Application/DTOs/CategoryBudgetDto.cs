

namespace ExpenseManagement.Application.DTOs
{
    public class CategoryBudgetDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal? MonthlyBudget { get; set; }
        public decimal MonthlyExpense { get; set; }
        public decimal ? RemainingBudget => MonthlyBudget.HasValue ? MonthlyBudget - MonthlyExpense : (decimal?)null;
        public decimal? YearlyBudget { get; set; }
        public decimal YearlyExpense { get; set; }
        public decimal ? RemainingYearlyBudget => YearlyBudget.HasValue ? YearlyBudget - YearlyExpense : (decimal?)null;

        public bool IsOverBudget => (MonthlyBudget.HasValue && MonthlyExpense > MonthlyBudget) || (YearlyBudget.HasValue && YearlyExpense > YearlyBudget);
        public int PendingExpensesCount { get; set; }
        public int ApprovedExpensesCount { get; set; }
        public decimal PendingExpensesAmount { get; set; }
        public decimal ApprovedExpensesAmount { get; set; }
    }
}
