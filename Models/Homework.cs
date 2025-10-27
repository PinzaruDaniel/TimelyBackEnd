namespace TimelyBackEnd.Models
{
    public class Homework
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
    }
}