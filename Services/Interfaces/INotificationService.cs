using TimelyBackEnd.DTOs.Notification;

namespace TimelyBackEnd.Services.Interfaces
{
    public interface INotificationService
    {
        Task ScheduleNotificationAsync(CreateNotificationDto dto);
        Task<List<NotificationDto>> GetPendingNotificationsAsync();
        Task SendNotificationAsync(CreateNotificationDto dto);
        Task RegisterFcmTokenAsync(Guid userId, string token);
        Task SendChatNotificationAsync(Guid senderId, Guid recipientId, string message);
        Task SendGroupChatNotificationAsync(Guid senderId, Guid groupId, string message);
    }
}