using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserLockout;

public class UpdateUserLockoutCommandHandler : IRequestHandler<UpdateUserLockoutCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public UpdateUserLockoutCommandHandler(UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserLockoutCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                user.IsLockedOut = request.IsLockedOut;
                user.LockedUntil = request.LockedUntil;
                await _userManager.UpdateAsync(user);
            }

            return Result.Success();
        }, cancellationToken);
    }
}