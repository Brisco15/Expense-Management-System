using ExpenseManagement.Application.DTOs;

namespace ExpenseManagement.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryExpenseDto>> GetAllCategoriesAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false);
        Task<CategoryExpenseDto?> GetCategoryByIdAsync(int id);
        Task<CategoryExpenseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, int userId);
        Task<CategoryExpenseDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, int id);
        Task<bool> SoftDeleteCategoryAsync(int id);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
