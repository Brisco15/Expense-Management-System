

namespace ExpenseManagement.Application.DTOs
{
    public class ApproveExpenseDto
    {
        public int ExpenseId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
