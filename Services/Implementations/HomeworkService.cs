using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Data;
using TimelyBackEnd.DTOs.Homework;
using TimelyBackEnd.Models;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Services.Implementations;

public class HomeworkService : IHomeworkService
{
    private readonly TimelyDbContext _context;

    public HomeworkService(TimelyDbContext context)
    {
        _context = context;
    }

    public async Task<HomeworkDto> AddHomeworkAsync(CreateHomeworkDto dto, Guid userId)
    {
        var group = await _context.Groups.FindAsync(dto.GroupId);
        if (group == null)
        {
            throw new Exception("Group not found.");
        }
        // Only owner can add homework (public or private)
        //TODO: to uncomment this!!!
       /*  if (group.OwnerId == null || group.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only the group owner can add homework to this group.");
        }  */
        
        // Handle DueDate - accept date-only (time is optional)
        // If timezone offset is provided, we still just use the date part
        DateTime? dueDateUtc = null;
        if (dto.DueDate.HasValue)
        {
            // Extract just the date part (ignore time)
            var dateOnly = dto.DueDate.Value.Date;
            // Store as midnight UTC
            dueDateUtc = DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
        }
        
        var homework = new Homework
        {
            GroupId = dto.GroupId,
            CreatedById = userId,
            Subject = dto.Subject,
            Description = dto.Description,
            DueDate = dueDateUtc,
            ImageUrl = dto.ImageUrl
        };
        _context.Homeworks.Add(homework);
        await _context.SaveChangesAsync();
        var createdByUser = await _context.Users.FindAsync(userId);
        var createdByName = createdByUser?.FullName ?? "Unknown";
        
        // Convert DateTime? to DateOnly? for response
        DateOnly? dueDateOnly = homework.DueDate.HasValue 
            ? DateOnly.FromDateTime(homework.DueDate.Value) 
            : null;
        
        return new HomeworkDto(homework.Id, homework.Subject, homework.Description, homework.CreatedAt, dueDateOnly, createdByName, homework.ImageUrl);
    }

    public async Task<List<HomeworkDto>> GetHomeworksForGroupAsync(Guid groupId)
    {
        var homeworks = await _context.Homeworks
            .Where(h => h.GroupId == groupId)
            .Include(h => h.CreatedBy)
            .ToListAsync();
        
        return homeworks.Select(h => 
        {
            DateOnly? dueDateOnly = h.DueDate.HasValue 
                ? DateOnly.FromDateTime(h.DueDate.Value) 
                : null;
            return new HomeworkDto(h.Id, h.Subject, h.Description, h.CreatedAt, dueDateOnly, h.CreatedBy.FullName, h.ImageUrl);
        }).ToList();
    }

    public async Task MarkHomeworkDoneAsync(Guid homeworkId)
    {
        var hw = await _context.Homeworks.FindAsync(homeworkId)
            ?? throw new Exception("Homework not found.");

        _context.Homeworks.Remove(hw);
        await _context.SaveChangesAsync();
    }
}