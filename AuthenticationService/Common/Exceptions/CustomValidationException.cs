using FluentValidation.Results;

namespace AuthenticationService.Common.Exceptions;

public class CustomValidationException : Exception
{
    public CustomValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public CustomValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures.Select(f => f.ErrorMessage);
    }

    public IEnumerable<string> Errors { get; }
}