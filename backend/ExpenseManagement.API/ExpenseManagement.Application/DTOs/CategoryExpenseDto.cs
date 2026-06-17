
namespace ExpenseManagement.Application.DTOs
{
    public class CategoryExpenseDto
    {
        public int CategoryId { get; set; } 
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; } 

        public decimal? MonthlyBudget { get; set; } = decimal.Zero;
        public decimal? YearlyBudget { get; set; } = decimal.Zero;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
