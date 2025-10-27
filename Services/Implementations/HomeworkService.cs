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

    public async Task<HomeworkDto> AddHomeworkAsync(CreateHomeworkDto dto)
    {
        var homework = new Homework
        {
            GroupId = dto.GroupId,
            Subject = dto.Subject,
            Description = dto.Description,
            DueDate = dto.DueDate,
            Status = "pending"
        };

        _context.Homeworks.Add(homework);
        await _context.SaveChangesAsync();

        return new HomeworkDto(homework.Id, homework.Subject, homework.Description, homework.DueDate, homework.Status);
    }

    public async Task<List<HomeworkDto>> GetHomeworksForGroupAsync(Guid groupId)
    {
        return await _context.Homeworks
            .Where(h => h.GroupId == groupId)
            .Select(h => new HomeworkDto(h.Id, h.Subject, h.Description, h.DueDate, h.Status))
            .ToListAsync();
    }

    public async Task MarkHomeworkDoneAsync(Guid homeworkId)
    {
        var hw = await _context.Homeworks.FindAsync(homeworkId)
            ?? throw new Exception("Homework not found.");

        hw.Status = "done";
        await _context.SaveChangesAsync();
    }
}