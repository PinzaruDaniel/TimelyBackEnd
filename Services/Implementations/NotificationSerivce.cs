using Quartz;
using TimelyBackEnd.DTOs.Notification;
using TimelyBackEnd.Models;
using TimelyBackEnd.Data;
using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly TimelyDbContext _context;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly FcmService _fcmService;

        public NotificationService(TimelyDbContext context, ISchedulerFactory schedulerFactory, FcmService fcmService)
        {
            _context = context;
            _schedulerFactory = schedulerFactory;
            _fcmService = fcmService;
        }

        public async Task SendNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                Title = dto.Title,
                Message = dto.Message,
                SendAt = dto.SendAt,
                GroupId = dto.GroupId,
                UserId = dto.UserId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // If SendAt is now or past, we might want to send it immediately via FCM as well
            if (dto.SendAt <= DateTime.UtcNow)
            {
                if (dto.UserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(dto.UserId.Value);
                    if (user != null && !string.IsNullOrEmpty(user.FcmToken))
                    {
                        await _fcmService.SendNotificationAsync(user.FcmToken, dto.Title, dto.Message);
                        notification.IsSent = true;
                        await _context.SaveChangesAsync();
                        return;
                    }
                }
                else if (dto.GroupId.HasValue)
                {
                    var tokens = await _context.Users
                        .Where(u => u.GroupId == dto.GroupId.Value && !string.IsNullOrEmpty(u.FcmToken))
                        .Select(u => u.FcmToken!)
                        .ToListAsync();
                    
                    if (tokens.Any())
                    {
                        await _fcmService.SendGroupNotificationAsync(tokens, dto.Title, dto.Message);
                        notification.IsSent = true;
                        await _context.SaveChangesAsync();
                        return;
                    }
                }
            }

            await ScheduleNotificationAsync(dto);
        }

        public async Task RegisterFcmTokenAsync(Guid userId, string token)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.FcmToken = token;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SendChatNotificationAsync(Guid senderId, Guid recipientId, string message)
        {
            var sender = await _context.Users.FindAsync(senderId);
            var recipient = await _context.Users.FindAsync(recipientId);

            if (recipient != null && !string.IsNullOrEmpty(recipient.FcmToken))
            {
                var title = $"New message from {sender?.FullName ?? "Someone"}";
                var data = new Dictionary<string, string>
                {
                    { "type", "chat" },
                    { "senderId", senderId.ToString() }
                };
                await _fcmService.SendNotificationAsync(recipient.FcmToken, title, message, data);
            }
        }

        public async Task SendGroupChatNotificationAsync(Guid senderId, Guid groupId, string message)
        {
            var sender = await _context.Users.FindAsync(senderId);
            var group = await _context.Groups.FindAsync(groupId);

            var tokens = await _context.Users
                .Where(u => u.GroupId == groupId && u.Id != senderId && !string.IsNullOrEmpty(u.FcmToken))
                .Select(u => u.FcmToken!)
                .ToListAsync();

            if (tokens.Any())
            {
                var title = $"Group {group?.Name ?? "Chat"}: {sender?.FullName ?? "Someone"}";
                var data = new Dictionary<string, string>
                {
                    { "type", "groupChat" },
                    { "groupId", groupId.ToString() },
                    { "senderId", senderId.ToString() }
                };
                await _fcmService.SendGroupNotificationAsync(tokens, title, message, data);
            }
        }

        public async Task ScheduleNotificationAsync(CreateNotificationDto dto)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var job = JobBuilder.Create<Jobs.NotificationJob>()
                .WithIdentity($"NotificationJob-{Guid.NewGuid()}")
                .Build();

            var trigger = TriggerBuilder.Create()
                .StartAt(dto.SendAt.ToUniversalTime())
                .WithSimpleSchedule(x => x.WithMisfireHandlingInstructionFireNow())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task<List<NotificationDto>> GetPendingNotificationsAsync()
        {
            var pending = await _context.Notifications
                .Where(n => !n.IsSent)
                .Select(n => new NotificationDto(
                    n.Id, n.Title, n.Message, n.SendAt, n.IsSent, n.GroupId, n.UserId
                ))
                .ToListAsync();

            return pending;
        }
    }
}