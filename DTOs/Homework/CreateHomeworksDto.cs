namespace TimelyBackEnd.DTOs.Homework
{
    public class CreateHomeworkDto
    {
        public Guid GroupId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }
}