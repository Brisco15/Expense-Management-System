using ExpenseManagement.Application.DTOs;
using System.IO;

namespace ExpenseManagement.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<List<ExpenseDto>> GetAllExpensesAsync();
        Task<ExpenseDto?> GetExpenseByIdAsync(int id);
        Task<List<ExpenseDto>> GetUserExpensesAsync(int userId);
        Task<ExpenseDto> CreateExpenseAsync( CreateExpenseDto createExpenseDto,int userId );

        Task<ExpenseDto> UpdateExpenseAsync(UpdateExpenseDto updateExpenseDto,int userId);
        
        Task<string?> UploadReceiptAsync(int id, Stream receiptStream, string fileName,int userId);

        Task<List<ExpenseDto>> GetExpensesByCategoryAsync(int categoryId, int userId);
        Task<List<ExpenseDto>> GetPendingExpensesAsync(int userId);
        Task<BudgetOverviewDto?> GetBudgetOverviewAsync(int userId);
        Task<ExpenseDto?> ApproveExpenseAsync(ApproveExpenseDto approveExpenseDto, int userId);

        Task<bool> DeleteExpenseAsync(int id);
        Task<bool> DeleteReceiptAsync(int id, int userId);
    }
}
