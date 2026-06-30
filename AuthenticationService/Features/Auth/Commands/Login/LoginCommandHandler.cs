using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.CreateRefreshToken;
using AuthenticationService.Features.Auth.Queries.CheckPassword;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    private readonly IGrpcIntegrationService _grpcIntegrationService;
    private readonly IJwtProvider _jwtProvider;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IMediator mediator,
        IJwtProvider jwtProvider,
        IGrpcIntegrationService grpcIntegrationService,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _jwtProvider = jwtProvider;
        _grpcIntegrationService = grpcIntegrationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var user = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);
            if (user == null)
                return Result<LoginCommandResponse>.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials,
                    "Invalid credentials."));

            if (user.IsLockedOut && user.LockedUntil > DateTime.UtcNow)
                return Result<LoginCommandResponse>.Failure(Error.Failure(AuthErrorCodes.AccountLocked,
                    "Account is locked. Please try again later."));

            var isPasswordValid = await _mediator.Send(new CheckPasswordQuery(user, request.Password), cancellationToken);
            if (!isPasswordValid)
                return Result<LoginCommandResponse>.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials,
                    "Invalid credentials."));

            var (accessToken, expiresIn) = _jwtProvider.GenerateToken(user);
            var refreshTokenString = _jwtProvider.GenerateRefreshToken();

            await _mediator.Send(new CreateRefreshTokenCommand(user.Id, refreshTokenString, DateTime.UtcNow.AddDays(7)),
                cancellationToken);

            var profileCompleted = await _grpcIntegrationService.HasCompletedProfileAsync(user.Id);
            var isPremium = await _grpcIntegrationService.IsPremiumUserAsync(user.Id);

            var response = new LoginCommandResponse(accessToken, refreshTokenString, profileCompleted, isPremium);
            return Result<LoginCommandResponse>.Success(response);
        }, cancellationToken);
    }
}