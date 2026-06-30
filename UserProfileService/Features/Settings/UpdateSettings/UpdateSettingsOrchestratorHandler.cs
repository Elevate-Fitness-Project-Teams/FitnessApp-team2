using AuthenticationService.Data;
using MediatR;
using UserProfileService.Common;
using UserProfileService.Features.Settings.UpdateSettings.UpdateNotificationSettings;
using UserProfileService.Features.Settings.UpdateSettings.UpdatePrivacySettings;
using UserProfileService.Features.Settings.UpdateSettings.UpdateUserPreferences;

namespace UserProfileService.Features.Settings.UpdateSettings;

public class UpdateSettingsOrchestratorHandler : IRequestHandler<UpdateSettingsOrchestrator, Result>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSettingsOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateSettingsOrchestrator request, CancellationToken cancellationToken)
    {
        Result? failureResult = null;

        try
        {
            await _unitOfWork.ExecuteAsync(async () =>
            {
                if (request.UserPreferences is not null)
                {
                    var dto = request.UserPreferences;
                    var result = await _mediator.Send(new UpdateUserPreferencesCommand(
                        request.UserId,
                        dto.Language,
                        dto.Theme,
                        dto.WeightUnit,
                        dto.HeightUnit,
                        dto.DistanceUnit
                    ), cancellationToken);

                    if (result.IsFailure)
                    {
                        failureResult = result;
                        throw new InvalidOperationException("Rollback");
                    }
                }

                if (request.NotificationSettings is not null)
                {
                    var dto = request.NotificationSettings;
                    var result = await _mediator.Send(new UpdateNotificationSettingsCommand(
                        request.UserId,
                        dto.WorkoutReminders,
                        dto.MealReminders,
                        dto.AchievementAlerts,
                        dto.WeeklyReports,
                        dto.EmailNotifications,
                        dto.PushNotifications
                    ), cancellationToken);

                    if (result.IsFailure)
                    {
                        failureResult = result;
                        throw new InvalidOperationException("Rollback");
                    }
                }

                if (request.PrivacySettings is not null)
                {
                    var dto = request.PrivacySettings;
                    var result = await _mediator.Send(new UpdatePrivacySettingsCommand(
                        request.UserId,
                        dto.ProfileVisibility,
                        dto.ShowProgressToFriends,
                        dto.AllowDataSharing
                    ), cancellationToken);

                    if (result.IsFailure)
                    {
                        failureResult = result;
                        throw new InvalidOperationException("Rollback");
                    }
                }
            }, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex) when (ex.Message == "Rollback" && failureResult != null)
        {
            return failureResult;
        }
    }
}
