using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserLockout;

public class UpdateUserLockoutCommandHandler : IRequestHandler<UpdateUserLockoutCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserLockoutCommandHandler(UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserLockoutCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                user.IsLockedOut = request.IsLockedOut;
                user.LockedUntil = request.LockedUntil;
                await _userManager.UpdateAsync(user);
            }
        }, cancellationToken);
    }
}