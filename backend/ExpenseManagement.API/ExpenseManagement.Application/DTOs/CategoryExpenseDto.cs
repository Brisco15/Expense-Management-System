
namespace ExpenseManagement.Application.DTOs
{
    public class CategoryExpenseDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } = decimal.Zero;
    }
}
