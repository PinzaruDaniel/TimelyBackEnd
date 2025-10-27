using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Models;

namespace TimelyBackEnd.Data;

public class TimelyDbContext : DbContext
{
    public TimelyDbContext(DbContextOptions<TimelyDbContext> options) : base(options) { }

    // Users (students)
    public DbSet<User> Users { get; set; } = null!;

    // Groups
    public DbSet<Group> Groups { get; set; } = null!;

    // Homework
    public DbSet<Homework> Homeworks { get; set; } = null!;

    // Schedule entries
    public DbSet<ScheduleEntry> ScheduleEntries { get; set; } = null!;

    // Notifications
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User <-> Group (many-to-one)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Group)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // ScheduleEntry <-> Group (many-to-one)
        modelBuilder.Entity<ScheduleEntry>()
            .HasOne(s => s.Group)
            .WithMany(g => g.ScheduleEntries)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Homework <-> Group (many-to-one)
        modelBuilder.Entity<Homework>()
            .HasOne(h => h.Group)
            .WithMany(g => g.Homeworks)
            .HasForeignKey(h => h.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Homework <-> User (many-to-one)
        modelBuilder.Entity<Homework>()
            .HasOne(h => h.CreatedBy)
            .WithMany(u => u.Homeworks)
            .HasForeignKey(h => h.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        // Notification relationships
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Group)
            .WithMany(g => g.Notifications)
            .HasForeignKey(n => n.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}