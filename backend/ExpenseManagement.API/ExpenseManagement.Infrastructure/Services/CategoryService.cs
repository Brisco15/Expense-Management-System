using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagement.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
    }

    private static CategoryExpenseDto MapToDto(Category category)
        {
            return new CategoryExpenseDto
            {
                CategoryId = category.Id,
                CategoryName= category.Name,
                Description = category.Description,
                MonthlyBudget = category.MonthlyBudget,
                YearlyBudget = category.YearlyBudget,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                IsActive = category.IsActive,

            }
        }
}
