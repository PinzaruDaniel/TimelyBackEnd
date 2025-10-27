namespace TimelyBackEnd.DTOs.Notification
{
    public record CreateNotificationDto(
        string Title,
        string Message,
        DateTime SendAt,
        Guid? GroupId,
        Guid? UserId
    );
}