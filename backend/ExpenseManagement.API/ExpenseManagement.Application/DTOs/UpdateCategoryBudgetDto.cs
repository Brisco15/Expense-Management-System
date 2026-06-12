

using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Application.DTOs
{
    public class UpdateCategoryBudgetDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Monthly budget must be non-negative")]
        public decimal? MonthlyBudget { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Yearly budget must be non-negative")]
        public decimal? YearlyBudget { get; set; }
    }
}
