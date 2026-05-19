namespace TimelyBackEnd.DTOs.Homework
{
    public class CreateHomeworkDto
    {
        public Guid GroupId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Optional due date. Time part is ignored - only the date is used.
        /// </summary>
        public DateTime? DueDate { get; set; }
        public Guid UserId { get; set; }
        /// <summary>
        /// Optional image URL for the homework photo.
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}