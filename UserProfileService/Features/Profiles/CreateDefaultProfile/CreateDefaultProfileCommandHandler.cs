using MediatR;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Models;

namespace UserProfileService.Features.Profiles.CreateDefaultProfile;

public class CreateDefaultProfileCommandHandler : IRequestHandler<CreateDefaultProfileCommand, Result>
{
    private readonly IGenericRepository<UserProfile> _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDefaultProfileCommandHandler(IGenericRepository<UserProfile> userProfileRepository, IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result> Handle(CreateDefaultProfileCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var userProfile = new UserProfile
            {
                Id = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                MemberSince = request.RegisteredAt,
                UserPreferences = new UserPreferences
                {
                    Id = request.UserId
                },
                NotificationSettings = new NotificationSettings
                {
                    Id = request.UserId
                },
                PrivacySettings = new PrivacySettings
                {
                    Id = request.UserId
                }
            };
            await _userProfileRepository.AddAsync(userProfile, cancellationToken);
            return Result.Success();
        }, cancellationToken);
    }
}
