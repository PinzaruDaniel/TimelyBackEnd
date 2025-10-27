using Quartz;
using Quartz.Impl;

namespace TimelyBackEnd.Services.Jobs;

public static class QuartzSchedulerService
{
    public static async Task<IScheduler> StartSchedulerAsync()
    {
        var factory = new StdSchedulerFactory();
        var scheduler = await factory.GetScheduler();
        await scheduler.Start();

        Console.WriteLine("🕒 Quartz Scheduler started.");
        return scheduler;
    }
}