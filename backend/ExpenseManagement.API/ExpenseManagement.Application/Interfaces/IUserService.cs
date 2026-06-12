using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Domain.Enums;

namespace ExpenseManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserRoleAsync(int userId, Role role);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<bool> DeleteUserAsync(int userId);
    }
}
