namespace TimelyBackEnd.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SendAt { get; set; }
        public bool IsSent { get; set; } = false;

        public Guid? GroupId { get; set; }
        public Group? Group { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}