namespace TimelyBackEnd.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // Student, Teacher, Admin

        // Additional fields for user data
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Age { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Country { get; set; }
        public string? ImageUrl { get; set; }

        public Guid? GroupId { get; set; }
        public Group? Group { get; set; }

        public ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public string? FcmToken { get; set; }
    }
}