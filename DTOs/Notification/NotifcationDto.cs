namespace TimelyBackEnd.DTOs.Notification
{
    public record NotificationDto(
        Guid Id,
        string Title,
        string Message,
        DateTime SendAt,
        bool IsSent,
        Guid? GroupId,
        Guid? UserId
    );
}