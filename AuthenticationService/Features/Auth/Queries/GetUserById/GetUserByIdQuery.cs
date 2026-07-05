using AuthenticationService.Common;
using AuthenticationService.Models.Responses;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetUserById;

public record GetUserByIdQuery(string UserId) : IRequest<Result<UserDto>>;