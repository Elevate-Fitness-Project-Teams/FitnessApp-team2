
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Models;

namespace UserProfileService.Features.Profiles.UploadProfilePicture;

public class UploadProfilePictureHandler : IRequestHandler<UploadProfilePictureCommand, Result<string>>
{
    private readonly IGenericRepository<UserProfile> _userProfileRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IUnitOfWork _unitOfWork;

    public UploadProfilePictureHandler(
        IGenericRepository<UserProfile> userProfileRepository,
        IWebHostEnvironment environment,
        IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _environment = environment;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            // 1. Build the file save path

            var extension = Path.GetExtension(request.ProfilePicture.FileName).ToLowerInvariant();
            var fileName = $"{request.UserId}{extension}";


            var folderPath = Path.Combine(_environment.WebRootPath, "images", "profiles");


            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            // 2. Save the uploaded file to disk

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.ProfilePicture.CopyToAsync(stream, cancellationToken);
            }

            // 3. Build the relative URL that clients will use to load the image

            var relativeUrl = $"/images/profiles/{fileName}";

            // 4. Update the database record with the new profile picture URL
            int affectedRows = await _userProfileRepository.GetQueryable()
                .Where(p => p.Id == request.UserId)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.ProfilePictureUrl, relativeUrl), cancellationToken);

            if (affectedRows == 0)
                return Result<string>.Failure(Error.NotFound("ProfileNotFound", "User profile not found."));

            return Result<string>.Success(relativeUrl);


        }, cancellationToken);
    }
}
