namespace TimelyBackEnd.DTOs.Homework
{
    public record HomeworkDto(
        Guid Id,
        string Subject,
        string Description,
        DateTime CreatedAt,
        DateTime? DueDate,
        string CreatedBy
    );
}