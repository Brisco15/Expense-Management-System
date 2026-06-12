
using ExpenseManagement.Domain.Enums;

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
        public int CategoryId { get; set; }

        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
        public string CreatedBy { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ReceiptPath { get; set; }
        public string? ReceiptFileName { get; set; }
        public bool HasReceipt {  get; set; }

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }

        
    }
}
