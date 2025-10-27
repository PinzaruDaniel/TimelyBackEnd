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

        public NotificationService(TimelyDbContext context, ISchedulerFactory schedulerFactory)
        {
            _context = context;
            _schedulerFactory = schedulerFactory;
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

            await ScheduleNotificationAsync(dto);
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