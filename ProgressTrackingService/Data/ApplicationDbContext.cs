using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<WorkoutLogExercise> WorkoutLogExercises => Set<WorkoutLogExercise>();
    public DbSet<WeightHistory> WeightHistories => Set<WeightHistory>();
    public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Streak> Streaks => Set<Streak>();
    public DbSet<UserStatistic> UserStatistics => Set<UserStatistic>();
    public DbSet<UserBiometrics> UserBiometrics => Set<UserBiometrics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}