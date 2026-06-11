using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;


namespace ExpenseManagement.Infrastructure.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;
        private readonly string _uploadPath;

        public ExpenseService(AppDbContext context)
        {
            _context = context;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "receipts");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        private static ExpenseDto MapToDto(Expense expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                Title = expense.Title,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                Status = expense.Status.ToString(),
                Category = expense.Category?.Name ?? string.Empty,
                CategoryId = expense.CategoryId,
                CreatedBy = expense.User?.FullName ?? string.Empty,
                CreatedByUserId = expense.UserId,
                CreatedAt = expense.CreatedAt,
                ReceiptPath = expense.ReceiptPath,
                ReceiptFileName = expense.ReceiptFilename,
                HasReceipt = !string.IsNullOrEmpty(expense.ReceiptPath),
                ApprovedBy = expense.ApprovedBy?.FullName,
                ApprovedAt = expense.ApprovedAt,
                RejectionReason = expense.RejectionReason
            };
        }


        /*    Gets Methods     */
        public async Task<List<ExpenseDto>> GetAllExpensesAsync()
        {
            var expenses = await _context.Expenses
                            .Include(e => e.User)
                            .Include(e => e.Category)
                            .Include(e => e.ApprovedBy)
                            .OrderByDescending(e => e.CreatedAt)
                            .ToListAsync();

            return expenses.Select(MapToDto).ToList();
            
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            var expense = await _context.Expenses
                            .Include(e => e.User)
                            .Include(e => e.Category)
                            .Include(e => e.ApprovedBy)
                            .FirstOrDefaultAsync(e => e.Id == id);

            return expense == null ? null : MapToDto(expense);

        }

        public async Task<List<ExpenseDto>> GetUserExpensesAsync(string userId)
        {
            var expenses = await _context.Expenses
                            .Where(e => e.UserId.ToString() == userId)
                            .Include(e => e.Category)
                            .Include(e => e.ApprovedBy)
                            .OrderByDescending(e => e.CreatedAt)
                            .ToListAsync();
            return expenses.Select(MapToDto).ToList();
        }

        public async Task<List<ExpenseDto>> GetExpensesByCategoryAsync(int categoryId, int userId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.CategoryId == categoryId && e.UserId == userId)
                .Include(e => e.Category)
                .Include(e => e.ApprovedBy)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            return expenses.Select(MapToDto).ToList();
        }

        public async Task<List<ExpenseDto>> GetPendingExpensesAsync(int userId)
        {
            var expenses = await _context.Expenses
                .Where(e => e.Status == ExpenseStatus.Pending)
                .Include(e => e.User)
                .Include(e => e.Category)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return expenses.Select(MapToDto).ToList();
        }

        public async Task<BudgetOverviewDto?> GetBudgetOverviewAsync(int userId)
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var userExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .ToListAsync();

            var monthlyExpenses = userExpenses
                .Where(e => e.ExpenseDate.Month == currentMonth &&
                           e.ExpenseDate.Year == currentYear &&
                           e.Status != ExpenseStatus.Rejected)
                .ToList();

            var yearlyExpenses = userExpenses
                .Where(e => e.ExpenseDate.Year == currentYear &&
                           e.Status != ExpenseStatus.Rejected)
                .ToList();

            var pendingExpenses = userExpenses
                .Where(e => e.Status == ExpenseStatus.Pending)
                .ToList();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var categoryBudgets = categories.Select(cat =>
            {
                var categoryMonthlyExpenses = monthlyExpenses
                    .Where(e => e.CategoryId == cat.Id)
                    .Sum(e => e.Amount);

                var monthlyRemaining = (cat.MonthlyBudget ?? 0) - categoryMonthlyExpenses;

                var categoryYearlyExpenses = yearlyExpenses
                    .Where(e => e.CategoryId == cat.Id)
                    .Sum(e => e.Amount);
                var yearlyRemaining = (cat.YearlyBudget ?? 0) - categoryYearlyExpenses;


                return new CategoryBudgetDto
                {
                    CategoryId = cat.Id,
                    CategoryName = cat.Name,
                    MonthlyBudget = cat.MonthlyBudget,
                    MonthlyExpense = categoryMonthlyExpenses,
                    YearlyBudget = cat.YearlyBudget,
                    YearlyExpense = categoryYearlyExpenses,
                    RemainingMonthlyBudget = monthlyRemaining,
                    RemainingYearlyBudget = yearlyRemaining,
                    IsMonthlyOverBudget = cat.MonthlyBudget.HasValue && categoryMonthlyExpenses > cat.MonthlyBudget.Value,
                    IsYearlyOverBudget = cat.YearlyBudget.HasValue && categoryYearlyExpenses > cat.YearlyBudget.Value
                };
            }).ToList();

            return new BudgetOverviewDto
            {
                TotalExpensesThisMonth = monthlyExpenses.Sum(e => e.Amount),
                TotalExpensesThisYear = yearlyExpenses.Sum(e => e.Amount),
                PendingExpensesCount = pendingExpenses.Count,
                PendingExpensesAmount = pendingExpenses.Sum(e => e.Amount),
                CategoryBudgets = categoryBudgets
            };
        }

        /*    Create Method     */

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto,int userId)
        {
            var expense = new Expense
            {
                Title = createExpenseDto.Title,
                Amount = createExpenseDto.Amount,
                ExpenseDate = createExpenseDto.ExpenseDate,
                Description = createExpenseDto.Description,
                CategoryId = createExpenseDto.CategoryId,
                UserId = userId,
                Status = ExpenseStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return await GetExpenseByIdAsync(expense.Id)
                ?? throw new Exception("Failed to create expense.");
        }

        /*    Update Method     */

        public async Task<ExpenseDto> UpdateExpenseAsync(UpdateExpenseDto updateExpenseDto, int userId)
        {
            var expense = await _context.Expenses.FindAsync(updateExpenseDto.Id);

            if (expense == null) 
            { 
                throw new Exception("Expense not found."); 
            }

            if (expense.UserId != userId) 
            { 
                throw new UnauthorizedAccessException("Unauthorized to update this expense."); 
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot update expense with status: {expense.Status}");
            }

            expense.Title = updateExpenseDto.Title;
            expense.Amount = updateExpenseDto.Amount;
            expense.ExpenseDate = updateExpenseDto.ExpenseDate;
            expense.Description = updateExpenseDto.Description;
            expense.UpdatedAt = DateTime.UtcNow;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
            return await GetExpenseByIdAsync(expense.Id)
                ?? throw new Exception("Failed to update expense.");
        }

        /*    Approve Method     */

        public async Task<ExpenseDto?> ApproveExpenseAsync(ApproveExpenseDto approveExpenseDto, int approverId)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == approveExpenseDto.ExpenseId);

            if (expense == null)
            {
                return null;
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot approve/reject expense with status: {expense.Status}");
            }

            if (approveExpenseDto.IsApproved)
            {
                expense.Status = ExpenseStatus.Approved;
                expense.ApprovedByUserId = approverId;
                expense.ApprovedAt = DateTime.UtcNow;
                expense.RejectionReason = null;
            }
            else
            {
                expense.Status = ExpenseStatus.Rejected;
                expense.ApprovedByUserId = approverId;
                expense.ApprovedAt = DateTime.UtcNow;
                expense.RejectionReason = approveExpenseDto.RejectionReason;
            }

            expense.UpdatedAt = DateTime.UtcNow;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            return await GetExpenseByIdAsync(expense.Id);
        }

        /*    Upload  Method     */

        public async Task<string?> UploadReceiptAsync(int id, Stream receiptStream, string fileName, int userId)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if(expense == null)
            {
                return null;
            }

            if(expense.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only upload receipts your own expenses ");
            }

            if (!string.IsNullOrEmpty(expense.ReceiptPath))
            {
                var oldFilePath = Path.Combine(_uploadPath, expense.ReceiptPath);
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{expense.Id}_{Guid.NewGuid()}{fileExtension}";
            var filepath = Path.Combine(_uploadPath, uniqueFileName);

            using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await receiptStream.CopyToAsync(stream);
            }

            expense.ReceiptPath = uniqueFileName;
            expense.ReceiptFilename = fileName;
            expense.ReceiptUploadedAt = DateTime.UtcNow;
            expense.UpdatedAt = DateTime.UtcNow;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            return uniqueFileName;
        }

        /*    Delete Methods     */

        public async Task<bool> DeleteReceiptAsync(int id, int userId)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if(expense== null)
            {
                return false;
            }

            if(expense.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete receipts for your own expenses.");
            }

            if (string.IsNullOrEmpty(expense.ReceiptPath))
            {
                return false;
            }

            var filePath = Path.Combine(_uploadPath, expense.ReceiptPath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            expense.ReceiptPath = null;
            expense.ReceiptFilename = null;
            expense.ReceiptUploadedAt = null;
            expense.UpdatedAt = DateTime.UtcNow;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if(expense == null) 
            { 
                return false; 
            }

            if (!string.IsNullOrEmpty(expense.ReceiptPath))
            {
                var filePath = Path.Combine(_uploadPath, expense.ReceiptPath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
