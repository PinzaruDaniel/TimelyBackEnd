using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimelyBackEnd.Data;
using TimelyBackEnd.DTOs.Auth;
using TimelyBackEnd.Models;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly TimelyDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(TimelyDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new Exception("Email already registered.");

        var (refreshToken, refreshExpiresAt) = GenerateRefreshToken();

        var user = new User
        {
            FullName = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = GenerateAccessToken(user),
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var (refreshToken, refreshExpiresAt) = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshExpiresAt;
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = GenerateAccessToken(user),
            RefreshToken = refreshToken
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new Exception("Refresh token is required.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiresAt.HasValue &&
            u.RefreshTokenExpiresAt > DateTime.UtcNow);

        if (user == null)
        {
            throw new Exception("Invalid or expired refresh token.");
        }

        return new RefreshTokenResponseDto
        {
            AccessToken = GenerateAccessToken(user)
        };
    }

    public async Task<UserDataDto> GetUserDataAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        return new UserDataDto
        {
            Id = 1, // As per requirement, using 1 for the default user
            FirstName = user.FirstName ?? "Alice",
            LastName = user.LastName ?? "Smith",
            Age = user.Age ?? 25,
            Email = user.Email,
            Address = new AddressDto
            {
                Street = user.Street ?? "123 Maple Street",
                City = user.City ?? "Springfield",
                State = user.State ?? "IL",
                Zip = user.Zip ?? "62701",
                Country = user.Country ?? "USA"
            },
            ImageUrl = user.ImageUrl ?? "https://optimistdrinks.com/cdn/shop/articles/oip21_day_5_1.jpg?v=1621112229"
        };
    }

    private string GenerateAccessToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
            new Claim(ClaimTypes.Email, user.Email)
        };

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // TODO: adjust the refresh token lifetime (in minutes) if needed
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("typ", "refresh")
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (refreshToken, expiresAt);
    }
}