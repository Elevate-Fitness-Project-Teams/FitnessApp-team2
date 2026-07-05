using AuthenticationService.Common;
using AuthenticationService.Models.Responses;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

public record RegisterOrchestrator(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber) : IRequest<Result<RegisterResponse>>;