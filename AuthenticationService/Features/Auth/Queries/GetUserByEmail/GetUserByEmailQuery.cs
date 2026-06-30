using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<User?>;