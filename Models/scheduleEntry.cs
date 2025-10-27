namespace TimelyBackEnd.Models
{
    public class ScheduleEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public string DayOfWeek { get; set; } = string.Empty; // "Luni", "Marți", ...
        public string Time { get; set; } = string.Empty;      // "08:00-09:30"
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Period { get; set; } = "every_week";    // "odd_week", "even_week", "every_week"
    }
}