using ExpenseManagement.Application.DTOs;
using ExpenseManagement.Application.Interfaces;
using ExpenseManagement.Domain.Entities;
using ExpenseManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
                
                return null;
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, workFactor:10),
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
            return await GenerateTokenAsync(user);
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

            if (!user.IsActive)
            {
                return null;
            }
            // Verify the password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return await GenerateTokenAsync(user);
        }

        // Refresh the access token using a valid refresh token
        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            // Find the user by refresh token
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);

            if (user == null)
            {
                return null;
            }

            // Check if the refresh token has expired
            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            
            if (!user.IsActive)
            {
                return null;
            }

            // Generate new tokens
            return await GenerateTokenAsync(user);
        }

        // Generate a JWT token and refresh token for the authenticated user
        private async Task<AuthResponseDto> GenerateTokenAsync(User user)
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

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            
            // Get refresh token expiration from configuration (default 7 days)
            var refreshTokenExpirationDaysString = _configuration["Jwt:RefreshTokenExpirationDays"];
            var refreshTokenExpirationDays = int.TryParse(refreshTokenExpirationDaysString, out var parsedDays) ? parsedDays : 7;

            // Update user with new refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return an authentication response containing both tokens
            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Email = user.Email,
                Role = user.Role.ToString(),
                ExpiresAt = token.ValidTo,
                ExpiresIn = (int)(token.ValidTo - DateTime.UtcNow).TotalSeconds
            };
        }

        // Generate a cryptographically secure random refresh token
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
