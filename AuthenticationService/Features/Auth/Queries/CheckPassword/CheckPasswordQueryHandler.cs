using AuthenticationService.Common;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Queries.CheckPassword;

public class CheckPasswordQueryHandler : IRequestHandler<CheckPasswordQuery, Result<bool>>
{
    private readonly UserManager<User> _userManager;

    public CheckPasswordQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(CheckPasswordQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<bool>.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials, "Invalid credentials."));

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        return Result<bool>.Success(isPasswordValid);
    }
}