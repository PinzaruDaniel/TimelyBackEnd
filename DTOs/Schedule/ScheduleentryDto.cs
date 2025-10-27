namespace TimelyBackEnd.DTOs.Schedule
{
    public record ScheduleEntryDto(
        string DayOfWeek,
        string Time,
        string Subject,
        string Teacher,
        string Room,
        string Period
    );
}