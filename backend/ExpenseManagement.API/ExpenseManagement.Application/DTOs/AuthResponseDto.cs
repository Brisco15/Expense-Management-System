
namespace ExpenseManagement.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public int ExpiresIn { get; set; }
    }
}
