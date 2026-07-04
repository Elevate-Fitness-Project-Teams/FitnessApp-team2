using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.CreateRefreshToken;
using AuthenticationService.Features.Auth.Commands.LogLoginAttempt;
using AuthenticationService.Features.Auth.Commands.UpdateUserLockout;
using AuthenticationService.Features.Auth.Queries.CheckPassword;
using AuthenticationService.Features.Auth.Queries.GetRecentFailedLoginAttempts;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Login;

public class LoginOrchestratorHandler : IRequestHandler<LoginOrchestrator, Result<LoginResponse>>
{
    private readonly IGrpcIntegrationService _grpcIntegrationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtProvider _jwtProvider;
    private readonly IMediator _mediator;
    public LoginOrchestratorHandler(
        IMediator mediator,
        IJwtProvider jwtProvider,
        IGrpcIntegrationService grpcIntegrationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _jwtProvider = jwtProvider;
        _grpcIntegrationService = grpcIntegrationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<LoginResponse>> Handle(LoginOrchestrator request,
        CancellationToken cancellationToken)
    {
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

        var userResult = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);

        if (!userResult.IsSuccess)
        {
            await _mediator.Send(new LogLoginAttemptCommand(request.Email, false, ipAddress), cancellationToken);
            return Result<LoginResponse>.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials,
                "Invalid credentials."));
        }

        var user = userResult.Value;
        if (user.IsLockedOut && user.LockedUntil > DateTime.UtcNow)
            return Result<LoginResponse>.Failure(Error.Failure(AuthErrorCodes.AccountLocked,
                "Account is locked. Please try again later."));

        if (!user.EmailConfirmed)
            return Result<LoginResponse>.Failure(Error.Failure(AuthErrorCodes.EmailNotConfirmed,
                "Email not confirmed. Please verify your email."));

        var isPasswordValid = await _mediator.Send(new CheckPasswordQuery(request.Email, request.Password), cancellationToken);

        if (!isPasswordValid.Value)
        {
            await _mediator.Send(new LogLoginAttemptCommand(request.Email, false, ipAddress), cancellationToken);

            var cutoffTime = DateTime.UtcNow.AddMinutes(-15);
            var recentFailuresCount =
                await _mediator.Send(new GetRecentFailedLoginAttemptsQuery(request.Email, cutoffTime),
                    cancellationToken);

            if (recentFailuresCount.Value >= 5)
            {
                await _mediator.Send(new UpdateUserLockoutCommand(request.Email, true, DateTime.UtcNow.AddMinutes(15)), cancellationToken);

                return Result<LoginResponse>.Failure(Error.Failure(AuthErrorCodes.AccountLocked,
                    "Account is locked due to multiple failed login attempts."));
            }

            return Result<LoginResponse>.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials,
                "Invalid credentials."));
        }

        await _mediator.Send(new LogLoginAttemptCommand(request.Email, true, ipAddress), cancellationToken);

        if (user.IsLockedOut)
        {
            await _mediator.Send(new UpdateUserLockoutCommand(request.Email, false, null), cancellationToken);
        }

        var (accessToken, expiresIn) = _jwtProvider.GenerateToken(user);
        var refreshTokenString = _jwtProvider.GenerateRefreshToken();

        await _mediator.Send(new CreateRefreshTokenCommand(user.Id, refreshTokenString, DateTime.UtcNow.AddDays(7)),
            cancellationToken);

        var profileCompleted = await _grpcIntegrationService.HasCompletedProfileAsync(user.Id);
        var isPremium = await _grpcIntegrationService.IsPremiumUserAsync(user.Id);

        var response = new LoginResponse(accessToken, refreshTokenString, expiresIn, profileCompleted, isPremium);
        return Result<LoginResponse>.Success(response);
    }
}