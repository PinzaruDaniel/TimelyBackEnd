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

        var schedule = new Schedule
        {
            GroupId = dto.GroupId,
            Days = dto.Days
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task<ScheduleDto?> GetScheduleByGroupAsync(Guid groupId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.GroupId == groupId);
        return schedule == null ? null : new ScheduleDto(schedule.Id, schedule.GroupId, schedule.Days);
    }
}