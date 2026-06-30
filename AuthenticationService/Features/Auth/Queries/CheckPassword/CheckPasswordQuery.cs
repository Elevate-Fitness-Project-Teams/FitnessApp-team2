using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.CheckPassword;

public record CheckPasswordQuery(User User, string Password) : IRequest<bool>;