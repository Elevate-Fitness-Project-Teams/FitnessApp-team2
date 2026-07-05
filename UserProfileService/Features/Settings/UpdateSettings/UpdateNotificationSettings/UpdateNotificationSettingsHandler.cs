
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Models;

namespace UserProfileService.Features.Settings.UpdateSettings.UpdateNotificationSettings;

public class UpdateNotificationSettingsHandler : IRequestHandler<UpdateNotificationSettingsCommand, Result>
{
    private readonly IGenericRepository<NotificationSettings> _repository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateNotificationSettingsHandler(IGenericRepository<NotificationSettings> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var rows = await _repository.GetQueryable()
         .Where(ns => ns.Id == request.UserId)
         .ExecuteUpdateAsync(setter => setter
         .SetProperty(ns => ns.WorkoutReminders, ns => request.WorkoutReminders ?? ns.WorkoutReminders)
         .SetProperty(ns => ns.MealReminders, ns => request.MealReminders ?? ns.MealReminders)
         .SetProperty(ns => ns.AchievementAlerts, ns => request.AchievementAlerts ?? ns.AchievementAlerts)
         .SetProperty(ns => ns.WeeklyReports, ns => request.WeeklyReports ?? ns.WeeklyReports)
         .SetProperty(ns => ns.EmailNotifications, ns => request.EmailNotifications ?? ns.EmailNotifications)
         .SetProperty(ns => ns.PushNotifications, ns => request.PushNotifications ?? ns.PushNotifications),
          cancellationToken);

            if (rows == 0)
                return Result.Failure(Error.NotFound("NotificationSettingsNotFound", "Notification settings not found."));

            return Result.Success();
        }, cancellationToken);
    }
}
