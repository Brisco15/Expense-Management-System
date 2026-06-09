

namespace ExpenseManagement.Application.DTOs
{
    public class UpdateExpenseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        
    }
}
