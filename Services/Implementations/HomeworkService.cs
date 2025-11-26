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
        var homework = new Homework
        {
            GroupId = dto.GroupId,
            CreatedById = userId,
            Subject = dto.Subject,
            Description = dto.Description,
            DueDate = dto.DueDate
        };
        _context.Homeworks.Add(homework);
        await _context.SaveChangesAsync();
        var createdByUser = await _context.Users.FindAsync(userId);
        var createdByName = createdByUser?.FullName ?? "Unknown";
        return new HomeworkDto(homework.Id, homework.Subject, homework.Description, homework.CreatedAt, homework.DueDate, createdByName);
    }

    public async Task<List<HomeworkDto>> GetHomeworksForGroupAsync(Guid groupId)
    {
        return await _context.Homeworks
            .Where(h => h.GroupId == groupId)
            .Include(h => h.CreatedBy)
            .Select(h => new HomeworkDto(h.Id, h.Subject, h.Description, h.CreatedAt, h.DueDate, h.CreatedBy.FullName))
            .ToListAsync();
    }

    public async Task MarkHomeworkDoneAsync(Guid homeworkId)
    {
        var hw = await _context.Homeworks.FindAsync(homeworkId)
            ?? throw new Exception("Homework not found.");

        _context.Homeworks.Remove(hw);
        await _context.SaveChangesAsync();
    }
}