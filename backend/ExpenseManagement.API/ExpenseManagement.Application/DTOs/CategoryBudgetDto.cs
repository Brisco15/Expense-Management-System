

namespace ExpenseManagement.Application.DTOs
{
    public class CategoryBudgetDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal? MonthlyBudget { get; set; }
        public decimal MonthlyExpense { get; set; }
        public decimal? RemainingMonthlyBudget { get; set; }
        public decimal? YearlyBudget { get; set; }
        public decimal YearlyExpense { get; set; }
        public decimal? RemainingYearlyBudget { get; set; }

        public bool? IsMonthlyOverBudget  {get; set;}
        public bool? IsYearlyOverBudget  {get; set;}
        public int PendingExpensesCount { get; set; }
        public int ApprovedExpensesCount { get; set; }
        public decimal PendingExpensesAmount { get; set; }
        public decimal ApprovedExpensesAmount { get; set; }
    }
}
