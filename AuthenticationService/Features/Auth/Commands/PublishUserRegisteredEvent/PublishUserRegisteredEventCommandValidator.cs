using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.PublishUserRegisteredEvent;

public class PublishUserRegisteredEventCommandValidator : AbstractValidator<PublishUserRegisteredEventCommand>
{
    public PublishUserRegisteredEventCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("User ID is required.");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
            
        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("First name is required.");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Last name is required.");
            
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Phone number is required.");
    }
}
