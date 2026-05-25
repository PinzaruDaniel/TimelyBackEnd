using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Data;
using TimelyBackEnd.DTOs.Schedule;
using TimelyBackEnd.Models;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Services.Implementations;

public class ScheduleService : IScheduleService
{
    private readonly TimelyDbContext _context;

    public ScheduleService(TimelyDbContext context)
    {
        _context = context;
    }

    public async Task AddScheduleAsync(CreateScheduleDto dto)
    {
        var group = await _context.Groups.FindAsync(dto.GroupId)
            ?? throw new Exception("Group not found.");

        var scheduleEntries = dto.ScheduleEntries.Select(entry => new ScheduleEntry
        {
            GroupId = dto.GroupId,
            DayOfWeek = entry.DayOfWeek,
            Time = entry.Time,
            Subject = entry.Subject,
            Teacher = entry.Teacher,
            Room = entry.Room,
            Period = entry.Period
        }).ToList();

        _context.ScheduleEntries.AddRange(scheduleEntries);
        await _context.SaveChangesAsync();
    }

    public async Task<ScheduleDto?> GetScheduleByGroupAsync(Guid groupId)
    {
        var group = await _context.Groups
            .Include(g => g.ScheduleEntries)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return null;

        var scheduleEntries = group.ScheduleEntries.Select(se => new ScheduleEntryDto(
            se.DayOfWeek,
            se.Time,
            se.Subject,
            se.Teacher,
            se.Room,
            se.Period
        )).ToList();

        return new ScheduleDto
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            GroupName = group.Name,
            ScheduleEntries = scheduleEntries
        };
    }
}