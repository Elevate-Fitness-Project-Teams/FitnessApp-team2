using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.CreateOtp;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.SendOtp;

public class SendOtpOrchestratorHandler : IRequestHandler<SendOtpOrchestrator, Result<SendOtpResponse>>
{
    private readonly IMediator _mediator;

    public SendOtpOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<SendOtpResponse>> Handle(SendOtpOrchestrator request,
        CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);

        if (user == null)
            return Result<SendOtpResponse>.Success(new SendOtpResponse(request.Email, 600, 30));

        var result = await _mediator.Send(new CreateOtpCommand(request.Email), cancellationToken);

        if (result.IsFailure)
            return Result<SendOtpResponse>.Failure(result.Errors);

        // TODO: Publish event to send the email

        return Result<SendOtpResponse>.Success(new SendOtpResponse(request.Email, 600, 30));
    }
}