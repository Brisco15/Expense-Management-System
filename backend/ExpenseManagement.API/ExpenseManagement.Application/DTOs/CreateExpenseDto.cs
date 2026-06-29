using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Application.DTOs
{
    public class CreateExpenseDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(50, ErrorMessage = "Title cannot exceed 50 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999,999.99")]
        public decimal Amount { get; set; } = decimal.Zero;

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Valid category must be selected")]
        public int CategoryId { get; set; }
    }
}
