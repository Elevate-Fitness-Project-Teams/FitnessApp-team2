using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.PublishUserRegisteredEvent;
using AuthenticationService.Features.Auth.Commands.Register;
using AuthenticationService.Features.Auth.Commands.SendOtp;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

public class RegisterOrchestratorHandler : IRequestHandler<RegisterOrchestrator, Result<RegisterOrchestratorResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterOrchestratorResponse>> Handle(RegisterOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var registerCommand = new RegisterCommand(request.Email, request.Password, request.FirstName,
                request.LastName, request.PhoneNumber);
            var registerResult = await _mediator.Send(registerCommand, cancellationToken);

            if (registerResult.IsFailure) return Result<RegisterOrchestratorResponse>.Failure(registerResult.Errors);

            var userId = registerResult.Value.UserId;

            var publishEventCommand = new PublishUserRegisteredEventCommand(userId, request.Email, request.FirstName,
                request.LastName, request.PhoneNumber);
            await _mediator.Send(publishEventCommand, cancellationToken);

            var otpResult = await _mediator.Send(new SendOtpCommand(request.Email), cancellationToken);
            if (otpResult.IsFailure) return Result<RegisterOrchestratorResponse>.Failure(otpResult.Errors);

            return Result<RegisterOrchestratorResponse>.Success(
                new RegisterOrchestratorResponse(userId, registerResult.Value.RequiresProfileCompletion));
        }, cancellationToken);
    }
}