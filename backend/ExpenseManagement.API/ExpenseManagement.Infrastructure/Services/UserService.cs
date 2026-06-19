
using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Domain.Enums;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagement.Infrastructure.Services
{
   
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 5, bool includeInactive = false)
        {
            var query = _context.Users.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();
            return new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await _context.Users
               .Where(u => u.Id == id)
               .Select(u => new UserDto
               {
                   Id = u.Id,
                   FullName = u.FullName,
                   Email = u.Email,
                   Role = u.Role.ToString(),
                   CreatedAt = u.CreatedAt,
                   UpdatedAt = u.UpdatedAt,
                   IsActive = u.IsActive
               })
               .FirstOrDefaultAsync();
        }

       
        public async Task<bool> UpdateUserRoleAsync(int userId, Role role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            if (user.Role == role)
                return true;

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (user.IsActive == isActive) return true;
            user.IsActive = isActive;

             user.UpdatedAt = DateTime.UtcNow;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            } 
            
        }
        
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Expenses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            if (user.Expenses.Any())
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    return false;
                }

            }

            _context.Users.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }

        }
    }
}
