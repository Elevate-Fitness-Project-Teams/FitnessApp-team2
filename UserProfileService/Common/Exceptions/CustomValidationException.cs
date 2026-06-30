using FluentValidation;

namespace UserProfileService.Common.Exceptions;

public class CustomValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public CustomValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public CustomValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures.Select(f => f.ErrorMessage);
    }
}
