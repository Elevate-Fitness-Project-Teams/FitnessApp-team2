using MassTransit;
using MassTransit.Testing;
using MediatR;
using MessageBroker.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Biometrics.Commands.UpsertUserBiometrics;
using ProgressTrackingService.Features.Users.Commands.InitializeUserProgress;
using ProgressTrackingService.MessageBroker.Consumers;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Consumers;

public class ConsumerIntegrationTests
{
    [Fact]
    public async Task UserRegisteredConsumer_WhenMessageReceived_DispatchesInitializeCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<InitializeUserProgressCommand, Result>(cmd =>
            {
                Assert.Equal(userId, cmd.UserId);
                return Task.FromResult(Result.Success());
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock);
        services.AddSingleton<ILogger<UserRegisteredConsumer>>(NullLogger<UserRegisteredConsumer>.Instance);
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);

        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(new UserRegisteredEvent
            {
                UserId = userId,
                Email = "athlete@elevate.com",
                FirstName = "John",
                LastName = "Doe"
            });

            // Assert
            Assert.True(await harness.Published.Any<UserRegisteredEvent>());
            var consumerHarness = harness.GetConsumerHarness<UserRegisteredConsumer>();
            Assert.True(await consumerHarness.Consumed.Any<UserRegisteredEvent>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task FitnessStatsSubmittedConsumer_WhenMessageReceived_DispatchesUpsertBiometricsCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var recordedAt = DateTime.UtcNow;

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<UpsertUserBiometricsCommand, Result>(cmd =>
            {
                Assert.Equal(userId, cmd.UserId);
                Assert.Equal(185.0, cmd.HeightCm);
                return Task.FromResult(Result.Success());
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock);
        services.AddSingleton<ILogger<FitnessStatsSubmittedConsumer>>(NullLogger<FitnessStatsSubmittedConsumer>.Instance);
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<FitnessStatsSubmittedConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);

        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(new FitnessStatsSubmittedEvent
            {
                UserId = userId,
                HeightCm = 185.0,
                WeightKg = 82.5,
                RecordedAt = recordedAt
            });

            // Assert
            Assert.True(await harness.Published.Any<FitnessStatsSubmittedEvent>());
            var consumerHarness = harness.GetConsumerHarness<FitnessStatsSubmittedConsumer>();
            Assert.True(await consumerHarness.Consumed.Any<FitnessStatsSubmittedEvent>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
