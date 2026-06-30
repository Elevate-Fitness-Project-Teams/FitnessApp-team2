using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

public record RegisterOrchestrator(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber) : IRequest<Result<RegisterOrchestratorResponse>>;

public record RegisterOrchestratorResponse(
    string UserId,
    bool RequiresProfileCompletion);