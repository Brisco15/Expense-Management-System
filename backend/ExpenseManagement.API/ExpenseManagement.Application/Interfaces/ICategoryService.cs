using ExpenseManagement.Application.DTOs;

namespace ExpenseManagement.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryExpenseDto>> GetAllCategoriesAsync();
        Task<CategoryExpenseDto?> GetCategoryByIdAsync(int id);
        Task<CategoryExpenseDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, int userId);
        Task<CategoryExpenseDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, int id);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
