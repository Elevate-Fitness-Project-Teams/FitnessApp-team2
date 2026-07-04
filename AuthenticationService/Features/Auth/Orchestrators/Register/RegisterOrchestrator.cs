using AuthenticationService.Common;
using MediatR;
using AuthenticationService.Models.Responses;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

public record RegisterOrchestrator(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber) : IRequest<Result<RegisterResponse>>;
