namespace TimelyBackEnd.DTOs.Schedule;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<ScheduleEntryDto> ScheduleEntries { get; set; } = new();
}
