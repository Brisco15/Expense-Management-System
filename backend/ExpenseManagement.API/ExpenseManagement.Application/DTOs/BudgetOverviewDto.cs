using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseManagement.Application.DTOs
{
    public class BudgetOverviewDto
    {
        public decimal TotalExpensesThisMonth { get; set; }
        public decimal TotalExpensesThisYear { get; set; }
        public int PendingExpensesCount { get; set; }
        public decimal PendingExpensesAmount { get; set; }
        public List<CategoryBudgetDto> CategoryBudgets { get; set; } = new List<CategoryBudgetDto>();


    }
}
