using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.CreateOtp;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.SendOtp;

public class SendOtpOrchestratorHandler : IRequestHandler<SendOtpOrchestrator, Result<SendOtpResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public SendOtpOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SendOtpResponse>> Handle(SendOtpOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var userResult = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);

            if (userResult.IsFailure)
                return Result<SendOtpResponse>.Success(new SendOtpResponse(request.Email, 600, 30));

            var result = await _mediator.Send(new CreateOtpCommand(request.Email), cancellationToken);

            if (result.IsFailure)
                return Result<SendOtpResponse>.Failure(result.Errors);

            // TODO: Publish event to send the email

            return Result<SendOtpResponse>.Success(new SendOtpResponse(request.Email, 600, 30));
        }, cancellationToken);
    }
}