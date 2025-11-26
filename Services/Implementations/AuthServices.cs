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

        var user = new User
        {
            FullName = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // TODO: Re-enable JWT token generation once authentication is restored.
        // var accessToken = GenerateAccessToken(user);
        // var refreshToken = await GenerateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            AccessToken = string.Empty,
            RefreshToken = string.Empty
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        // TODO: Re-enable JWT token generation once authentication is restored.
        // var accessToken = GenerateAccessToken(user);
        // var refreshToken = await GenerateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            AccessToken = string.Empty,
            RefreshToken = string.Empty
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Re-enable refresh token validation once JWT auth is restored.
        return new RefreshTokenResponseDto
        {
            AccessToken = string.Empty
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

    // TODO: Uncomment when JWT tokens are reintroduced.
    // private string GenerateAccessToken(User user) { ... }
    // private Task<string> GenerateRefreshTokenAsync(User user) { ... }
}