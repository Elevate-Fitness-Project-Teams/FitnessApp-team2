using FluentValidation;

namespace AuthenticationService.Features.Auth.Queries.GetUserById;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("User ID is required.");
    }
}
