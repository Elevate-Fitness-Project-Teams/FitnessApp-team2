using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetUserById;

public record GetUserByIdQuery(string UserId) : IRequest<User?>;