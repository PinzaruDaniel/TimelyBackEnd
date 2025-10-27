using TimelyBackEnd.Models;

namespace TimelyBackEnd.DTOs.Schedule;

public class CreateScheduleDto
{
    public Guid GroupId { get; set; }
    public List<CreateScheduleEntryDto> ScheduleEntries { get; set; } = new();
}

public class CreateScheduleEntryDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Period { get; set; } = "every_week";
}
