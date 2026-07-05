using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var user = new User
            {
                Id = request.UserId,
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsLockedOut = false,
                CreatedAt = DateTime.UtcNow
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors
                    .Select(e => Error.Failure(e.Code, e.Description))
                    .ToList();

                return Result.Failure(errors);
            }

            return Result.Success();
        }, cancellationToken);
    }
}