using FluentValidation;

namespace UserProfileService.Features.Profiles.UploadProfilePicture;

public class UploadProfilePictureValidator : AbstractValidator<UploadProfilePictureCommand>
{
    // 5 MB in bytes
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    // Allowed MIME types for the uploaded image
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png" };

    // Allowed file extensions
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

    public UploadProfilePictureValidator()
    {
        RuleFor(x => x.ProfilePicture)
            .NotNull().WithMessage("Profile picture is required.")
            .Must(file => file != null && file.Length > 0)
                .WithMessage("Profile picture file must not be empty.")
            .Must(file => file != null && file.Length <= MaxFileSizeInBytes)
                .WithMessage("Profile picture must not exceed 5 MB.")
            .Must(file => file != null && AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                .WithMessage("Only JPG and PNG image formats are allowed.")
            .Must(file =>
            {
                if (file == null) return false;
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                return !string.IsNullOrEmpty(extension) && AllowedExtensions.Contains(extension);
            })
                .WithMessage("File extension must be .jpg, .jpeg, or .png.");
    }
}
