using System.ComponentModel.DataAnnotations;

namespace TimelyBackEnd.DTOs.Notification
{
    public record CreateNotificationDto(
        [property: Required, MinLength(2)] string Title,
        [property: Required, MinLength(2)] string Message,
        DateTime SendAt,
        Guid? GroupId,
        Guid? UserId
    );
}