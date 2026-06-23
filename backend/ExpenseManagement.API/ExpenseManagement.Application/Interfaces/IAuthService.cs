using ExpenseManagement.Application.DTOs;

namespace ExpenseManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);

        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    }
}
