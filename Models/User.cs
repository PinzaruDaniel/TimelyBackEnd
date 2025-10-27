namespace TimelyBackEnd.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // Student, Teacher, Admin

        public Guid? GroupId { get; set; }
        public Group? Group { get; set; }

        public ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}