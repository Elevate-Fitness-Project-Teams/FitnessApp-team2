using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using AuthenticationService.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Queries.GetUserByEmail;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IGeneralRepo<User> _repo;

    public GetUserByEmailQueryHandler(IGeneralRepo<User> repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _repo.Find(u => u.Email == request.Email)
            .Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsLockedOut = user.IsLockedOut,
                LockedUntil = user.LockedUntil,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result<UserDto>.Failure(Error.Failure(AuthErrorCodes.UserNotFound,
                $"User with email {request.Email} not found."));

        return Result<UserDto>.Success(user);
    }
}