using ExpenseManagement.Application.DTOs;

namespace ExpenseManagement.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto> GetAllExpensesAsync();
        Task<ExpenseDto?> GetExpenseByIdAsync(int id);
        Task<ExpenseDto> CreateExpenseAsync(
            CreateExpenseDto createExpenseDto,
            int userId
            );
    }
}
