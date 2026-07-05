using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.PublishUserRegisteredEvent;
using AuthenticationService.Features.Auth.Commands.Register;
using AuthenticationService.Features.Auth.Orchestrators.SendOtp;
using AuthenticationService.Models.Responses;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

// Add Transaction
public class RegisterOrchestratorHandler : IRequestHandler<RegisterOrchestrator, Result<RegisterResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
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
        }, cancellationToken);
    }
}
