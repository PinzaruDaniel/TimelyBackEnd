using System.ComponentModel.DataAnnotations;
using TimelyBackEnd.Helpers;

namespace TimelyBackEnd.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [AllowedGroup]
    public string Group { get; set; } = string.Empty;
}
