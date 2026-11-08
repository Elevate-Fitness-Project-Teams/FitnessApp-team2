using AuthenticationService.Common;
using AuthenticationService.Data.Entities;
using AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;
using AuthenticationService.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Commands.ChangeUserPassword;

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, Result>
{
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;
    private readonly ITokenRevocationStore _tokenRevocationStore;

    public ChangeUserPasswordCommandHandler(
        UserManager<User> userManager,
        IMediator mediator,
        ITokenRevocationStore tokenRevocationStore)
    {
        _userManager = userManager;
        _mediator = mediator;
        _tokenRevocationStore = tokenRevocationStore;
    }

    public async Task<Result> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return Result.Failure(Error.NotFound("USER_NOT_FOUND", "User not found."));

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
            return Result.Failure(errors);
        }

        // 1. Revoke all active Refresh Tokens for this user in SQL Server
        await _mediator.Send(new RevokeAllRefreshTokensCommand(request.UserId), cancellationToken);

        // 2. Invalidate all previous JWT access tokens in Redis
        if (Guid.TryParse(request.UserId, out var userGuid))
        {
            await _tokenRevocationStore.RevokeAllUserTokensAsync(
                userGuid,
                DateTimeOffset.UtcNow,
                TimeSpan.FromMinutes(15),
                cancellationToken);
        }

        return Result.Success();
    }
}
