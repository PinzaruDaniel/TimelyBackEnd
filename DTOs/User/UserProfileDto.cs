namespace TimelyBackEnd.DTOs.User
{
    public record UserProfileDto(
        Guid Id,
        string FullName,
        string Email,
        string Role,
        string? FirstName,
        string? LastName,
        int? Age,
        string? Street,
        string? City,
        string? State,
        string? Zip,
        string? Country,
        string? ImageUrl,
        Guid? GroupId,
        string? GroupName);
}

