using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.Register;

/// <summary>
///     Registers a new user, publishes the integration event, and creates an OTP for email verification.
///     Returns Result (no data) — the controller owns the userId and constructs the API response.
/// </summary>
public record RegisterCommand(
    string UserId,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber) : IRequest<Result>;