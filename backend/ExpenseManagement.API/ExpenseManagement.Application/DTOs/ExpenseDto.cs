
namespace ExpenseManagement.Application.DTOs
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;
    }
}
