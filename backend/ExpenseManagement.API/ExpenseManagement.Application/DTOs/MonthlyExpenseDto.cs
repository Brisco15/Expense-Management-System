

namespace ExpenseManagement.Application.DTOs
{
    public class MonthlyExpenseDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } = decimal.Zero;
    }
}
