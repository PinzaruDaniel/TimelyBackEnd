using TimelyBackEnd.DTOs.Notification;

namespace TimelyBackEnd.Services.Interfaces
{
    public interface INotificationService
    {
        Task ScheduleNotificationAsync(CreateNotificationDto dto);
        Task<List<NotificationDto>> GetPendingNotificationsAsync();
        Task SendNotificationAsync(CreateNotificationDto dto);
    }
}