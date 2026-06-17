using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Application.DTOs
{
    public class UpdateCategoryDto
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "Monthly budget must be positive")]
        public decimal? MonthlyBudget { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "Yearly budget must be positive")]
        public decimal? YearlyBudget { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
