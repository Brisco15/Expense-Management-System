using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace ExpenseManagement.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }
    

    private static CategoryExpenseDto MapToDto(Category category)
        {
            // Exclude rejected expenses from aggregates for consistency with BudgetService
            var validExpenses = category.Expenses
                ?.Where(e => e.Status != ExpenseStatus.Rejected)
                .ToList();

            return new CategoryExpenseDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                Description = category.Description,
                MonthlyBudget = category.MonthlyBudget,
                YearlyBudget = category.YearlyBudget,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                IsActive = category.IsActive,
                TotalExpenses = validExpenses?.Count ?? 0,
                TotalAmount = validExpenses?.Sum(e => e.Amount) ?? 0,
                LastExpenseDate = validExpenses != null && validExpenses.Any()
                    ? validExpenses.Max(e => e.ExpenseDate)
                    : null
            };
        }

        /* Gets Methods */
        public async Task<PagedResult<CategoryExpenseDto>> GetAllCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            bool includeInactive = false)
        {
            var baseQuery = _context.Categories.AsQueryable();
            if (!includeInactive)
                baseQuery = baseQuery.Where(c => c.IsActive);

            var totalCount = await baseQuery.CountAsync();

            // Project aggregates in SQL — no expense rows transferred to application
            var categories = await baseQuery
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryExpenseDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    Description = c.Description,
                    MonthlyBudget = c.MonthlyBudget,
                    YearlyBudget = c.YearlyBudget,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsActive = c.IsActive,
                    TotalExpenses = c.Expenses.Count(e => e.Status != ExpenseStatus.Rejected),
                    TotalAmount = c.Expenses
                        .Where(e => e.Status != ExpenseStatus.Rejected)
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    LastExpenseDate = c.Expenses
                        .Where(e => e.Status != ExpenseStatus.Rejected)
                        .Max(e => (DateTime?)e.ExpenseDate)
                })
                .ToListAsync();

            return new PagedResult<CategoryExpenseDto>
            {
                Items = categories,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<CategoryExpenseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                            .Include(c => c.Expenses)
                            .FirstOrDefaultAsync(c => c.Id == id);
            return category == null ? null : MapToDto(category);
        }

        /* Create Method */

        public async Task<CategoryExpenseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, int userId)
        {
            _logger.LogInformation("Creating category {CategoryName} for user {UserId}",
                createCategoryDto.CategoryName, userId);

            try
            {
                var isDuplicate = await _context.Categories
                                   .AnyAsync(c => c.Name.ToLower() == createCategoryDto.CategoryName.ToLower());


                if (isDuplicate)
                {
                    throw new InvalidOperationException($"Category with name '{createCategoryDto.CategoryName}' already exists");
                }

                var category = new Category
                {

                    Name = createCategoryDto.CategoryName,
                    Description = createCategoryDto.Description,
                    MonthlyBudget = createCategoryDto.MonthlyBudget,
                    YearlyBudget = createCategoryDto.YearlyBudget,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created category {CategoryId}", category.Id);
                return await GetCategoryByIdAsync(category.Id)
                    ?? throw new Exception("Failed to retrieve created category"); ;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to create category {CategoryName}", createCategoryDto.CategoryName);
                throw;
            }
        }

        /* Delete Method */

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if(category == null)
            {
                return false;
            }

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                             .Include(c => c.Expenses)
                             .FirstOrDefaultAsync(c => c.Id == id);
                
            if (category == null) return false ;

            if (category.Expenses.Any())
            {
                throw new InvalidOperationException("Cannot delete category with existing expenses. Consider marking it as inactive.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;

        }

        /* Update Category */

        public async Task<CategoryExpenseDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, int id)
        {
            if (id != updateCategoryDto.CategoryId)
            {
                throw new ArgumentException("Category ID mismatch");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found");
            }

            // Check for duplicate name (excluding current category)
            var duplicateName = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == updateCategoryDto.CategoryName.ToLower() 
                            && c.Id != id);
            
            if (duplicateName)
            {
                throw new InvalidOperationException($"Category '{updateCategoryDto.CategoryName}' already exists");
            }

            category.Name = updateCategoryDto.CategoryName;
            category.Description = updateCategoryDto.Description;
            category.UpdatedAt = DateTime.UtcNow;
            category.MonthlyBudget = updateCategoryDto.MonthlyBudget;
            category.YearlyBudget = updateCategoryDto.YearlyBudget;
            category.IsActive = updateCategoryDto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            
            return await GetCategoryByIdAsync(category.Id)
                ?? throw new Exception("Failed to update category");
        }

    }
    }
