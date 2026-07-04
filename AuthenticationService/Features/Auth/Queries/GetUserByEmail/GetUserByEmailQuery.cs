using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetUserByEmail;

using AuthenticationService.Common;
using AuthenticationService.Models.Responses;

public record GetUserByEmailQuery(string Email) : IRequest<Result<UserDto>>;