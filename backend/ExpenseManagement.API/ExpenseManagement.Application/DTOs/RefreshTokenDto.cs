

namespace ExpenseManagement.Application.DTOs
{
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
