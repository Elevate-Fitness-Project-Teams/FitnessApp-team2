using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.CheckPassword;

public record CheckPasswordQuery(string Email, string Password) : IRequest<Result<bool>>;