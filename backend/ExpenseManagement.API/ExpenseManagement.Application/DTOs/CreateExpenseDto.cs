namespace ExpenseManagement.Application.DTOs
{
    public class CreateExpenseDto
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
