using ExpenseManagement.Domain.Enums;

namespace ExpenseManagement.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public ExpenseStatus Status { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        public string? ReceiptPath { get; set; }
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}