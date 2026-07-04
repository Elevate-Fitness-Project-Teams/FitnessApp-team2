using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.PublishUserRegisteredEvent;
using AuthenticationService.Features.Auth.Commands.Register;
using AuthenticationService.Features.Auth.Orchestrators.SendOtp;
using AuthenticationService.Models.Responses;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

public class RegisterOrchestratorHandler : IRequestHandler<RegisterOrchestrator, Result<RegisterResponse>>
{
    private readonly IMediator _mediator;

    public RegisterOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterOrchestrator request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid().ToString();

        var registerCommand = new RegisterCommand(
            userId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
            );

        var registerResult = await _mediator.Send(registerCommand, cancellationToken);

        if (registerResult.IsFailure)
        {
            return Result<RegisterResponse>.Failure(registerResult.Errors);
        }

        var publishEventCommand = new PublishUserRegisteredEventCommand(
            userId,
            request.Email,
            request.FirstName,
            request.LastName,
            request.PhoneNumber);
        await _mediator.Send(publishEventCommand, cancellationToken);

        var otpResult = await _mediator.Send(new SendOtpOrchestrator(request.Email), cancellationToken);
        if (otpResult.IsFailure) return Result<RegisterResponse>.Failure(otpResult.Errors);

        return Result<RegisterResponse>.Success(
            new RegisterResponse(userId, true));
    }
}
