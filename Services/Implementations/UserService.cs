using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Data;
using TimelyBackEnd.DTOs.User;
using TimelyBackEnd.Models;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Services.Implementations;

public class UserService : IUserService
{
    private readonly TimelyDbContext _context;

    public UserService(TimelyDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new Exception("User not found.");

        return MapProfile(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, string? imageUrl)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new Exception("User not found.");

        if (dto.FullName != null)
        {
            user.FullName = Normalize(dto.FullName) ?? string.Empty;
        }

        if (dto.FirstName != null)
        {
            user.FirstName = Normalize(dto.FirstName);
        }

        if (dto.LastName != null)
        {
            user.LastName = Normalize(dto.LastName);
        }

        if (dto.Age.HasValue)
        {
            user.Age = dto.Age;
        }

        if (dto.Street != null)
        {
            user.Street = Normalize(dto.Street);
        }

        if (dto.City != null)
        {
            user.City = Normalize(dto.City);
        }

        if (dto.State != null)
        {
            user.State = Normalize(dto.State);
        }

        if (dto.Zip != null)
        {
            user.Zip = Normalize(dto.Zip);
        }

        if (dto.Country != null)
        {
            user.Country = Normalize(dto.Country);
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            user.ImageUrl = imageUrl;
        }

        await _context.SaveChangesAsync();
        return MapProfile(user);
    }

    public async Task<IEnumerable<UserProfileDto>> GetUsersByGroupIdAsync(Guid groupId)
    {
        var users = await _context.Users
            .Where(u => u.GroupId == groupId)
            .ToListAsync();

        return users.Select(MapProfile);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static UserProfileDto MapProfile(User user)
    {
        return new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.FirstName,
            user.LastName,
            user.Age,
            user.Street,
            user.City,
            user.State,
            user.Zip,
            user.Country,
            user.ImageUrl,
            user.GroupId);
    }
}

