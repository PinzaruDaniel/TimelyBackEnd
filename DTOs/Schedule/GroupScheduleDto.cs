namespace TimelyBackEnd.DTOs.Schedule
{
    public class GroupScheduleDto
    {
        public string Group { get; set; } = string.Empty;
        public Dictionary<string, List<ScheduleEntryDto>> Days { get; set; } = new();
    }
}