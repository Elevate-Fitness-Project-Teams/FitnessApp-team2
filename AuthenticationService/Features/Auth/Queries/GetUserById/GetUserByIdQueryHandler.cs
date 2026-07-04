using AuthenticationService.Data.Entities;
using AuthenticationService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Queries.GetUserById;

using AuthenticationService.Models.Responses;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IGeneralRepo<User> _repo;

    public GetUserByIdQueryHandler(IGeneralRepo<User> repo)
    {
        _repo = repo;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repo.Find(u => u.Id == request.UserId)
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
    }
}