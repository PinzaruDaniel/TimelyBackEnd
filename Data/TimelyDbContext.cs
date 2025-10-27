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
    public DbSet<Homework> Homework { get; set; } = null!;

    // Schedule entries
    public DbSet<ScheduleEntry> ScheduleEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User <-> Group (many-to-one)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // ScheduleEntry <-> Group (many-to-one)
        modelBuilder.Entity<ScheduleEntry>()
            .HasOne(s => s.Group)
            .WithMany(g => g.ScheduleEntries)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Homework <-> ScheduleEntry (many-to-one)
        modelBuilder.Entity<Homework>()
            .HasOne(h => h.ScheduleEntry)
            .WithMany(s => s.Homework)
            .HasForeignKey(h => h.ScheduleEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Homework <-> User (many-to-one)
        modelBuilder.Entity<Homework>()
            .HasOne(h => h.Student)
            .WithMany(u => u.Homework)
            .HasForeignKey(h => h.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}