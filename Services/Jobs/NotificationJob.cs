using Quartz;
using TimelyBackEnd.Data;
using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Services.Implementations;

namespace TimelyBackEnd.Services.Jobs
{
    public class NotificationJob : IJob
    {
        private readonly TimelyDbContext _context;
        private readonly FcmService _fcmService;

        public NotificationJob(TimelyDbContext context, FcmService fcmService)
        {
            _context = context;
            _fcmService = fcmService;
        }
 
        public async Task Execute(IJobExecutionContext context)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsSent && n.SendAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var n in notifications)
            {
                // Send FCM notification
                if (n.UserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(n.UserId.Value);
                    if (user != null && !string.IsNullOrEmpty(user.FcmToken))
                    {
                        await _fcmService.SendNotificationAsync(user.FcmToken, n.Title, n.Message);
                    }
                }
                else if (n.GroupId.HasValue)
                {
                    var tokens = await _context.Users
                        .Where(u => u.GroupId == n.GroupId.Value && !string.IsNullOrEmpty(u.FcmToken))
                        .Select(u => u.FcmToken!)
                        .ToListAsync();

                    if (tokens.Any())
                    {
                        await _fcmService.SendGroupNotificationAsync(tokens, n.Title, n.Message);
                    }
                }

                Console.WriteLine($"📢 Sending notification: {n.Title} -> {n.Message}");

                n.IsSent = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}