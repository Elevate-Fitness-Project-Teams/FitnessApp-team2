using AuthenticationService.Common;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Commands.ConfirmUserEmail;

public class ConfirmUserEmailCommandHandler : IRequestHandler<ConfirmUserEmailCommand, Result>
{
    private readonly UserManager<User> _userManager;

    public ConfirmUserEmailCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user != null && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        return Result.Success();
    }
}
