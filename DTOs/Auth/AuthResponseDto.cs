namespace TimelyBackEnd.DTOs.Auth;

public record AuthResponseDto(Guid Id, string Name, string Email, string AccessToken, string RefreshToken, Guid? GroupId);
