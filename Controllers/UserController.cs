using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimelyBackEnd.DTOs.User;
using TimelyBackEnd.Services;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly ImageCompressionService _imageCompressionService;

    public UserController(
        IUserService userService,
        IWebHostEnvironment environment,
        ImageCompressionService imageCompressionService)
    {
        _userService = userService;
        _environment = environment;
        _imageCompressionService = imageCompressionService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdOrUnauthorized();
        if (userId == null)
        {
            return Unauthorized();
        }

        var profile = await _userService.GetProfileAsync(userId.Value);
        return Ok(profile);
    }

    [HttpPut("me")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UpdateMe([FromForm] UpdateUserProfileDto dto)
    {
        var userId = GetUserIdOrUnauthorized();
        if (userId == null)
        {
            return Unauthorized();
        }

        string? imageUrl = null;
        if (dto.Photo != null && dto.Photo.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(dto.Photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { error = "Invalid file type. Allowed types: jpg, jpeg, png, gif, webp" });
            }

            var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }

            var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "users");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            using (var inputStream = dto.Photo.OpenReadStream())
            {
                var (compressedStream, compressedExtension) = await _imageCompressionService.CompressImageAsync(
                    inputStream,
                    maxWidth: 1024,
                    maxHeight: 1024,
                    quality: 85
                );

                var fileName = $"{Guid.NewGuid()}{compressedExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await compressedStream.CopyToAsync(fileStream);
                }

                imageUrl = $"/uploads/users/{fileName}";
            }
        }

        var profile = await _userService.UpdateProfileAsync(userId.Value, dto, imageUrl);
        return Ok(profile);
    }

    private Guid? GetUserIdOrUnauthorized()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

