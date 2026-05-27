using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimelyBackEnd.Data;
using TimelyBackEnd.DTOs.Auth;
using TimelyBackEnd.Helpers;
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

        if (await _context.Users.AnyAsync(u => u.FullName == dto.Name))
            throw new Exception("Username (Full Name) already taken.");

        var groupName = AllowedGroups.Normalize(dto.Group);
        if (!AllowedGroups.Names.Contains(groupName))
        {
            throw new Exception("Invalid group.");
        }

        var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == groupName);
        if (group == null)
        {
            group = new Group
            {
                Name = groupName,
                SchoolName = "Default School"
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
        }

        var user = new User
        {
            FullName = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            GroupId = group.Id,
            Group = group
        };

        if (!group.Users.Any(u => u.Id == user.Id))
        {
            group.Users.Add(user);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var tokens = CreateTokenPair(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        await _context.SaveChangesAsync();

        return new AuthResponseDto(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, user.GroupId, group.Name);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var tokens = CreateTokenPair(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        await _context.SaveChangesAsync();

        return new AuthResponseDto(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, user.GroupId, user.Group?.Name);
    }

    public async Task<TokenPairDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new Exception("Refresh token is required.");

        var principal = GetPrincipalFromToken(refreshToken, validateLifetime: false);
        if (principal == null)
            throw new Exception("Invalid refresh token.");

        var tokenType = principal.FindFirst("token_type")?.Value;
        if (!string.Equals(tokenType, "refresh", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Invalid refresh token.");

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            throw new Exception("Invalid refresh token.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.RefreshToken != refreshToken)
            throw new Exception("Invalid refresh token.");

        if (user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            throw new Exception("Refresh token expired.");

        var tokens = CreateTokenPair(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        await _context.SaveChangesAsync();

        return new TokenPairDto(tokens.AccessToken, tokens.RefreshToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            throw new Exception("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _context.SaveChangesAsync();
    }

    private (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt) CreateTokenPair(User user)
    {
        var accessTokenMinutes = _config.GetValue("Jwt:AccessTokenMinutes", 15);
        var refreshTokenDays = _config.GetValue("Jwt:RefreshTokenDays", 30);

        var accessToken = GenerateJwtToken(user, TimeSpan.FromMinutes(accessTokenMinutes), "access");
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays);
        var refreshToken = GenerateJwtToken(user, TimeSpan.FromDays(refreshTokenDays), "refresh");

        return (accessToken, refreshToken, refreshTokenExpiresAt);
    }

    private string GenerateJwtToken(User user, TimeSpan lifetime, string tokenType)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("token_type", tokenType),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = key
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
