using TimelyBackEnd.DTOs.Schedule;

namespace TimelyBackEnd.Services.Interfaces;

public interface IScheduleService
{
    Task AddScheduleAsync(CreateScheduleDto dto);
    Task<ScheduleDto?> GetScheduleByGroupAsync(Guid groupId);
} 