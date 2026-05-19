using Microsoft.AspNetCore.Http;

namespace TimelyBackEnd.DTOs.Homework
{
    public class CreateHomeworkFormDto
    {
        public Guid GroupId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Optional due date as string (e.g., "2024-01-15"). Time part is ignored - only the date is used.
        /// </summary>
        public string? DueDate { get; set; }
        public Guid UserId { get; set; }
        /// <summary>
        /// Optional photo/image file for the homework.
        /// </summary>
        public IFormFile? Photo { get; set; }
    }
}

