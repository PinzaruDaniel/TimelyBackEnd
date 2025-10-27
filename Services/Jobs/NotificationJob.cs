using Quartz;
using TimelyBackEnd.Data;
using Microsoft.EntityFrameworkCore;

namespace TimelyBackEnd.Services.Jobs
{
    public class NotificationJob : IJob
    {
        private readonly TimelyDbContext _context;

        public NotificationJob(TimelyDbContext context)
        {
            _context = context;
        }
 
        public async Task Execute(IJobExecutionContext context)
        {
            var notifications = await _context.Notifications
                .Where(n => !n.IsSent && n.SendAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var n in notifications)
            {
                // Here you can send an email, push notification, etc.
                Console.WriteLine($"📢 Sending notification: {n.Title} -> {n.Message}");

                n.IsSent = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}