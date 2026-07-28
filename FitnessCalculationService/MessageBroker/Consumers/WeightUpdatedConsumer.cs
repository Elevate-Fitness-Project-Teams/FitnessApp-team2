using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MassTransit;
using MessageBroker.Events;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.MessageBroker.Consumers;

public class WeightUpdatedConsumer : IConsumer<WeightUpdatedEvent>
{
    private readonly ILogger<WeightUpdatedConsumer> _logger;
    private readonly IGenericRepository<UserFitnessStats> _statsRepo;

    public WeightUpdatedConsumer(IGenericRepository<UserFitnessStats> statsRepo, ILogger<WeightUpdatedConsumer> logger)
    {
        _statsRepo = statsRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WeightUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received WeightUpdatedEvent for User {UserId} with NewWeight {NewWeight}",
            message.UserId, message.NewWeight);

        var rowsAffected = await _statsRepo.GetQueryable()
            .Where(s => s.UserId == message.UserId.ToString())
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Weight, message.NewWeight),
                context.CancellationToken);

        if (rowsAffected == 0)
        {
            _logger.LogWarning(
                "No UserFitnessStats row found for User {UserId}. Weight not updated.",
                message.UserId);
        }
        else
        {
            _logger.LogInformation(
                "Successfully updated Weight to {NewWeight} for User {UserId}",
                message.NewWeight, message.UserId);
        }
    }
}
