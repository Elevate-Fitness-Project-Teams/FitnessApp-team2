using AuthenticationService.Common;
using AuthenticationService.Models.Responses;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<Result<UserDto>>;