using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ExpenseManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if the email is already registered
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                // User already exists
                return null;
            }

            // Create a new User entity and hash the password
            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                // Hash the password using BCrypt
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Expenses = new List<Expense>()
            };

            // Add the new user to the database
            _context.Users.Add(user);
            // Save changes to the database
            await _context.SaveChangesAsync();
            // Generate a JWT token for the newly registered user and return it
            return GenerateToken(user);

        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return GenerateToken(user);
        }

        private AuthResponseDto GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
                );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Role = user.Role.ToString(),
                ExpiresAt = token.ValidTo,
                ExpiresIn = (int)(token.ValidTo - DateTime.UtcNow).TotalSeconds
            };
        }
    }
}
