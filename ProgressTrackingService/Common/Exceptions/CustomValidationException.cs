using FluentValidation.Results;
using ProgressTrackingService.Common.Errors;

namespace ProgressTrackingService.Common.Exceptions;

public class CustomValidationException : Exception
{
    public CustomValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }

    public CustomValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures.Select(f =>
            !string.IsNullOrWhiteSpace(f.ErrorCode) && f.ErrorCode != f.PropertyName
                ? f.ErrorCode
                : (!string.IsNullOrWhiteSpace(f.ErrorMessage) ? f.ErrorMessage : ProgressErrors.ValRequiredField))
            .Distinct()
            .ToList();
    }

    public IEnumerable<string> Errors { get; }
}