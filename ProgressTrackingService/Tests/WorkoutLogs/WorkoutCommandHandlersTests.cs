using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;
using ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;
using ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateStatistic;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.WorkoutLogs;

public class WorkoutCommandHandlersTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly GeneralRepo<WorkoutLog> _workoutLogRepo;
    private readonly GeneralRepo<UserStatistic> _statsRepo;
    private readonly GeneralRepo<Streak> _streakRepo;
    private readonly UnitOfWork _uow;

    public WorkoutCommandHandlersTests()
    {
        _db = new SqliteTestDbContext();
        _workoutLogRepo = new GeneralRepo<WorkoutLog>(_db.Context);
        _statsRepo = new GeneralRepo<UserStatistic>(_db.Context);
        _streakRepo = new GeneralRepo<Streak>(_db.Context);
        _uow = new UnitOfWork(_db.Context);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task SaveWorkoutLogHandler_PersistsWorkoutLogWithExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var sessionId = "session-test-01";
        var completedAt = DateTime.UtcNow;

        var handler = new SaveWorkoutLogHandler(_workoutLogRepo);
        var command = new SaveWorkoutLogCommand
        {
            UserId = userId,
            WorkoutId = workoutId,
            SessionId = sessionId,
            CompletedAt = completedAt,
            Duration = 45,
            CaloriesBurned = 320,
            Difficulty = "Advanced",
            Notes = "Challenging leg session",
            Rating = 5,
            ExercisesCompleted = new List<SaveWorkoutLogExerciseDto>
            {
                new SaveWorkoutLogExerciseDto
                {
                    ExerciseId = exerciseId,
                    Sets = 4,
                    Reps = 12,
                    WeightUsed = 60.0,
                    Completed = true
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _uow.SaveChangesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var saved = await _db.Context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == result.Value);

        Assert.NotNull(saved);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(workoutId, saved.WorkoutId);
        Assert.Equal(sessionId, saved.SessionId);
        Assert.Equal(45, saved.DurationInMinutes);
        Assert.Equal(320, saved.CaloriesBurned);
        Assert.Equal(WorkoutDifficulty.Advanced, saved.Difficulty);
        Assert.Equal(5, saved.Rating);
        Assert.Single(saved.Exercises);

        var ex = saved.Exercises.First();
        Assert.Equal(exerciseId, ex.ExerciseId);
        Assert.Equal(4, ex.SetsCompleted);
        Assert.Equal(12, ex.RepsCompleted);
        Assert.Equal(60.0, ex.WeightUsed);
        Assert.True(ex.Completed);
    }

    [Fact]
    public async Task UpdateStatisticHandler_WhenUserDoesNotExist_CreatesInitialStatisticRecord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new UpdateStatisticHandler(_statsRepo, _uow);

        var command = new UpdateStatisticCommand
        {
            UserId = userId,
            CaloriesBurned = 400,
            DurationInMinutes = 50
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = await _db.Context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(stats);
        Assert.Equal(1, stats.TotalWorkouts);
        Assert.Equal(400, stats.TotalCaloriesBurned);
        Assert.Equal(50, stats.TotalMinutes);
    }

    [Fact]
    public async Task UpdateStatisticHandler_WhenCalledSequentially_AccumulatesTotalsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new UpdateStatisticHandler(_statsRepo, _uow);

        // Act - First workout
        await handler.Handle(new UpdateStatisticCommand
        {
            UserId = userId,
            CaloriesBurned = 300,
            DurationInMinutes = 30
        }, CancellationToken.None);

        _db.Context.ChangeTracker.Clear();

        // Act - Second workout
        await handler.Handle(new UpdateStatisticCommand
        {
            UserId = userId,
            CaloriesBurned = 250,
            DurationInMinutes = 25
        }, CancellationToken.None);

        // Assert
        var stats = await _db.Context.UserStatistics.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TotalWorkouts);
        Assert.Equal(550, stats.TotalCaloriesBurned);
        Assert.Equal(55, stats.TotalMinutes);
    }

    [Fact]
    public async Task UpdateStreakHandler_WhenNewStreak_CreatesStreakRecord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new UpdateStreakHandler(_streakRepo, _uow);
        var completedAt = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await handler.Handle(new UpdateStreakCommand
        {
            UserId = userId,
            CompletedAt = completedAt
        }, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.CurrentStreak);
        Assert.Equal(1, result.Value.LongestStreak);
        Assert.True(result.Value.StreakUpdated);

        var saved = await _db.Context.Streaks.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(saved);
        Assert.Equal(1, saved.CurrentStreak);
        Assert.Equal(1, saved.LongestStreak);
        Assert.Equal(completedAt, saved.LastWorkoutDate);
    }

    [Fact]
    public async Task UpdateStreakHandler_WhenConsecutiveDays_IncrementsStreakAndPreservesLongest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var day1 = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
        var day2 = new DateTime(2026, 10, 21, 11, 0, 0, DateTimeKind.Utc);

        _db.Context.Streaks.Add(new Streak
        {
            UserId = userId,
            CurrentStreak = 4,
            LongestStreak = 7,
            LastWorkoutDate = day1
        });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var handler = new UpdateStreakHandler(_streakRepo, _uow);

        // Act
        var result = await handler.Handle(new UpdateStreakCommand
        {
            UserId = userId,
            CompletedAt = day2
        }, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.CurrentStreak);
        Assert.Equal(7, result.Value.LongestStreak);
        Assert.True(result.Value.StreakUpdated);

        var updated = await _db.Context.Streaks.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(updated);
        Assert.Equal(5, updated.CurrentStreak);
        Assert.Equal(7, updated.LongestStreak);
        Assert.Equal(day2, updated.LastWorkoutDate);
    }
}
