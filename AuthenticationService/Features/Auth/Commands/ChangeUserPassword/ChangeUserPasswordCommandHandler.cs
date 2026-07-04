using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Commands.ChangeUserPassword;

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, Result>
{
    private readonly UserManager<User> _userManager;

    public ChangeUserPasswordCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure(Error.NotFound("USER_NOT_FOUND", "User not found."));
        }

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
            return Result.Failure(errors);
        }

        return Result.Success();
    }
}
