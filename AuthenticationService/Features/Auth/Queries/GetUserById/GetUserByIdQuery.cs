using AuthenticationService.Data.Entities;
using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetUserById;

using AuthenticationService.Models.Responses;

public record GetUserByIdQuery(string UserId) : IRequest<Result<UserDto>>;