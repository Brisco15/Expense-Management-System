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
        // Register a new user and return an authentication response with a JWT token
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if the email is already registered
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                // Email already exists
                return null;
            }

            // Create a new User entity and hash the password
            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = Domain.Enums.Role.Employee,
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
        // Authenticate a user and return an authentication response with a JWT token if successful
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            // If the user is not found, return null
            if (user == null)
            {
                return null;
            }
            // Verify the password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return GenerateToken(user);
        }
        // Generate a JWT token for the authenticated user
        private AuthResponseDto GenerateToken(User user)
        {
            // Create claims for the JWT token, including user ID, email, and role
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            // Retrieve the JWT key from configuration and create a symmetric security key
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured.");
            // Create signing credentials using the symmetric security key and HMAC SHA256 algorithm
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Retrieve the JWT issuer, audience, and expiration time from configuration
            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is not configured.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var expirationMinutesString = _configuration["Jwt:ExpirationMinutes"];
            var expirationMinutes = int.TryParse(expirationMinutesString, out var parsed) ? parsed : 15;

            // Create a JWT token with the specified issuer, audience, claims, expiration time, and signing credentials
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
                );
            // Return an authentication response containing
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
