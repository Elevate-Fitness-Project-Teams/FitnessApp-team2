using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Queries.CheckPassword;

public class CheckPasswordQueryHandler : IRequestHandler<CheckPasswordQuery, bool>
{
    private readonly UserManager<User> _userManager;

    public CheckPasswordQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(CheckPasswordQuery request, CancellationToken cancellationToken)
    {
        return await _userManager.CheckPasswordAsync(request.User, request.Password);
    }
}