using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                TotalExpenses = category.Expenses?.Count ?? 0,
                TotalAmount = category.Expenses?.Sum(e => e.Amount) ?? 0,
                LastExpenseDate = category.Expenses?.Max(e => e.ExpenseDate)

            };
        }

        /* Gets Methods */
        public async Task<PagedResult<CategoryExpenseDto>> GetAllCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            bool includeInactive = false)
        {
            var query = _context.Categories.AsQueryable();
            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var totalCount = await query.CountAsync();

            var categories = await query
                              .OrderByDescending(c => c.CreatedAt)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
            return new PagedResult<CategoryExpenseDto> 
            { 
                Items = categories.Select(MapToDto).ToList(),
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
                                   .AnyAsync(c => c.Name.Equals(createCategoryDto.CategoryName, StringComparison.OrdinalIgnoreCase));


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
