using ExpenseManagement.Domain.Enums;

namespace ExpenseManagement.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }


        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
        public string? Description { get; set; }


        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? ReceiptPath { get; set; }
        public string? ReceiptFilename { get; set; }
        public DateTime? ReceiptUploadedAt { get; set; }


        public int? ApprovedByUserId { get; set; }
        public User? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}