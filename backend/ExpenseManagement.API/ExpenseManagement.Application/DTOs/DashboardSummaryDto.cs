namespace ExpenseManagement.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalExpenses { get; set; } = decimal.Zero;
        public decimal TotalExpensesThisMonth { get; set; } = decimal.Zero;
        public decimal TotalExpensesThisYear { get; set; } = decimal.Zero;
        public int TotalExpenseCount { get; set; } = 0;
        public decimal ApprovedExpenses { get; set; } = decimal.Zero;
        public decimal PendingExpenses { get; set; } = decimal.Zero;
        public decimal RejectedExpenses { get; set; } = decimal.Zero;
        public int RejectedExpenseCount { get; set; } = 0;
        public List<RecentExpenseItem> RecentExpenses { get; set; } = new();
    }
}
