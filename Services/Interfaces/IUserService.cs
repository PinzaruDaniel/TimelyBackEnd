using TimelyBackEnd.DTOs.User;

namespace TimelyBackEnd.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto, string? imageUrl);
    Task<IEnumerable<UserProfileDto>> GetUsersByGroupIdAsync(Guid groupId);
}

