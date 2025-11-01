namespace TimelyBackEnd.Models
{
    public class Group
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // e.g. PAPP-231
        public string SchoolName { get; set; } = string.Empty;
        public string? InviteCode { get; set; } // null or empty for private groups

        public bool IsPrivate { get; set; } = false; // true if group is private
        public Guid? OwnerId { get; set; } // for private group owner

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
        public ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}