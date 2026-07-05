using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using AuthenticationService.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IGeneralRepo<User> _repo;

    public GetUserByIdQueryHandler(IGeneralRepo<User> repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repo.Find(u => u.Id == request.UserId)
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

        if (user == null)
            return Result<UserDto>.Failure(Error.Failure(AuthErrorCodes.UserNotFound,
                $"User with ID {request.UserId} not found."));

        return Result<UserDto>.Success(user);
    }
}