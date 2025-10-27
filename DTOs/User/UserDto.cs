namespace TimelyBackEnd.DTOs.User
{
    public record UserDto(Guid Id, string FullName, string Email, string Role, Guid? GroupId);
}