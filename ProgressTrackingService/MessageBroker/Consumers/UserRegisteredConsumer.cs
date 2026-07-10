using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace MessageBroker.Events;
public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IGeneralRepo<Streak> _streakRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        IGeneralRepo<Streak> streakRepo,
        IUnitOfWork unitOfWork,
        ILogger<UserRegisteredConsumer> logger)
    {
        _streakRepo = streakRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var userId = context.Message.UserId;
        _logger.LogInformation("Received UserRegisteredEvent for User {UserId}", userId);

        await _unitOfWork.ExecuteAsync(async () =>
        {
            var existing = await _streakRepo.Find(s => s.UserId == userId).FirstOrDefaultAsync(context.CancellationToken);
            if (existing == null)
            {
                var newStreak = new Streak
                {
                    UserId = userId,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastWorkoutDate = null
                };

                await _streakRepo.AddAsync(newStreak, context.CancellationToken);
                _logger.LogInformation("Initialized Streak for User {UserId}", userId);
            }
        }, context.CancellationToken);
    }
}
