using Quartz;
using TimelyBackEnd.Data;
using Microsoft.EntityFrameworkCore;

namespace TimelyBackEnd.Services.Jobs
{
    public class HomeworkCleanupJob : IJob
    {
        private readonly TimelyDbContext _context;

        public HomeworkCleanupJob(TimelyDbContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.UtcNow;
            var expiredHomeworks = await _context.Homeworks
                .Where(hw => hw.DueDate != null && hw.DueDate < now)
                .ToListAsync();

            if (expiredHomeworks.Count > 0)
            {
                _context.Homeworks.RemoveRange(expiredHomeworks);
                await _context.SaveChangesAsync();
                Console.WriteLine($"🧹 Deleted {expiredHomeworks.Count} expired homework(s)");
            }
        }
    }
}
