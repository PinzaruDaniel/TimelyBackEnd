using System.ComponentModel.DataAnnotations;

namespace TimelyBackEnd.DTOs.Schedule;

public class CreateScheduleDto
{
    [Required]
    public Guid GroupId { get; set; }

    [MinLength(1)]
    public List<CreateScheduleEntryDto> ScheduleEntries { get; set; } = new();
}

public class CreateScheduleEntryDto
{
    [Required]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required]
    public string Time { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string Teacher { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Period { get; set; } = "every_week";
}
