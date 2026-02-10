using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimelyBackEnd.DTOs.Homework;
using TimelyBackEnd.Services.Interfaces;
using TimelyBackEnd.Services;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeworkController : ControllerBase
{
    private readonly IHomeworkService _homeworkService;
    private readonly IWebHostEnvironment _environment;
    private readonly ImageCompressionService _imageCompressionService;

    public HomeworkController(
        IHomeworkService homeworkService, 
        IWebHostEnvironment environment,
        ImageCompressionService imageCompressionService)
    {
        _homeworkService = homeworkService;
        _environment = environment;
        _imageCompressionService = imageCompressionService;
    }

    [HttpPost("add")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB limit for original file (will be compressed)
    //[Authorize]
    public async Task<IActionResult> AddHomework([FromForm] CreateHomeworkFormDto dto)
    {
        string? imageUrl = null;

        // Handle photo upload if provided
        if (dto.Photo != null && dto.Photo.Length > 0)
        {
            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(dto.Photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { error = "Invalid file type. Allowed types: jpg, jpeg, png, gif, webp" });
            }

            // Create uploads directory if it doesn't exist
            var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }
            var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "homework");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Compress and save image
            // TODO: Adjust compression settings if needed:
            // - maxWidth/maxHeight: Maximum image dimensions (default: 1920x1920)
            // - quality: JPEG quality 1-100 (default: 85, higher = better quality but larger file)
            using (var inputStream = dto.Photo.OpenReadStream())
            {
                var (compressedStream, compressedExtension) = await _imageCompressionService.CompressImageAsync(
                    inputStream,
                    maxWidth: 1920,  // TODO: Adjust max width if needed
                    maxHeight: 1920, // TODO: Adjust max height if needed
                    quality: 85      // TODO: Adjust JPEG quality (1-100) if needed
                );

                // Generate unique filename with compressed extension
                var fileName = $"{Guid.NewGuid()}{compressedExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save compressed file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await compressedStream.CopyToAsync(fileStream);
                }

                // Generate URL
                imageUrl = $"/uploads/homework/{fileName}";
            }
        }

        // Parse DueDate if provided
        DateTime? dueDate = null;
        if (!string.IsNullOrWhiteSpace(dto.DueDate))
        {
            if (DateTime.TryParse(dto.DueDate, out var parsedDate))
            {
                dueDate = parsedDate;
            }
        }

        // Create DTO for service
        var createDto = new CreateHomeworkDto
        {
            GroupId = dto.GroupId,
            Subject = dto.Subject,
            Description = dto.Description,
            DueDate = dueDate,
            UserId = dto.UserId,
            ImageUrl = imageUrl
        };

        var homework = await _homeworkService.AddHomeworkAsync(createDto, dto.UserId);
        return Ok(homework);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetHomeworksForGroup(Guid groupId)
    {
        // Return mock homework for specific group ID
        if (groupId == Guid.Parse("cb41031e-f64e-484b-a38b-3ac7c6995c38"))
        {
            return Ok(BuildMockHomeworkResponse());
        }

        var homeworks = await _homeworkService.GetHomeworksForGroupAsync(groupId);
        return Ok(homeworks);
    }

    private static List<HomeworkDto> BuildMockHomeworkResponse()
    {
        return new List<HomeworkDto>
        {
            new HomeworkDto(
                Id: Guid.NewGuid(),
                Subject: "Matematica",
                Description: "Rezolvați exercițiile de la paginile 45-50",
                CreatedAt: DateTime.UtcNow.AddDays(-2),
                DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                CreatedBy: "V. Guci",
                ImageUrl: null
            ),
            new HomeworkDto(
                Id: Guid.NewGuid(),
                Subject: "Informatica",
                Description: "Proiect final - dezvoltarea unei aplicații web",
                CreatedAt: DateTime.UtcNow.AddDays(-5),
                DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                CreatedBy: "A. Pintea",
                ImageUrl: null
            ),
            new HomeworkDto(
                Id: Guid.NewGuid(),
                Subject: "Limba engleză",
                Description: "Scrieți un eseu despre tehnologia modernă (300-400 cuvinte)",
                CreatedAt: DateTime.UtcNow.AddDays(-1),
                DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                CreatedBy: "D. Leahu",
                ImageUrl: null
            ),
            new HomeworkDto(
                Id: Guid.NewGuid(),
                Subject: "Sisteme de gestiune a bazelor de date",
                Description: "Creați un model de date pentru un sistem de management",
                CreatedAt: DateTime.UtcNow.AddDays(-3),
                DueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                CreatedBy: "A. Pintea",
                ImageUrl: "/uploads/homework/3079c30d-a36d-4801-bf6c-f2c3b0f87316.png"
            )
        };
    }

    [HttpPut("{id}/done")]
        [Authorize]
    public async Task<IActionResult> MarkHomeworkDone(Guid id)
    {
        await _homeworkService.MarkHomeworkDoneAsync(id);
        return Ok(new { message = "Homework marked as done." });
    }
}