using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;
using AuthenticationService.Features.Auth.Orchestrators.RefreshToken;
using AuthenticationService.Features.Auth.Queries.GetRefreshToken;
using AuthenticationService.Models.Responses;
using AuthenticationService.Options;
using AuthenticationService.Services;
using Xunit;

namespace AuthenticationService.Tests;

public class RefreshTokenOrchestratorHandlerTests
{
    private readonly FakeMediator _mediator;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeTokenRevocationStore _tokenRevocationStore;
    private readonly JwtProvider _jwtProvider;
    private readonly RefreshTokenOrchestratorHandler _handler;

    public RefreshTokenOrchestratorHandlerTests()
    {
        _mediator = new FakeMediator();
        _unitOfWork = new FakeUnitOfWork();
        _tokenRevocationStore = new FakeTokenRevocationStore();

        var (privateKeyPath, publicKeyPath) = TestRsaKeyHelper.EnsureTestKeys();

        var options = new JwtOptions
        {
            PrivateKeyPath = privateKeyPath,
            PublicKeyPath = publicKeyPath,
            KeyId = "elevate-fitness-key-1",
            Issuer = "ElevateFitness",
            Audience = "ElevateFitnessUsers",
            ExpirationInMinutes = 15
        };
        _jwtProvider = new JwtProvider(Microsoft.Extensions.Options.Options.Create(options));

        _handler = new RefreshTokenOrchestratorHandler(
            _mediator,
            _jwtProvider,
            _unitOfWork,
            _tokenRevocationStore);
    }

    [Fact]
    public async Task Handle_WhenReplayingAlreadyRevokedRefreshToken_TriggersTheftRevocation()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var userId = userGuid.ToString();
        var stolenToken = "stolen-already-revoked-token";

        // Mock GetRefreshTokenQuery returning an ALREADY REVOKED token
        var revokedTokenDto = new RefreshTokenDto(
            10,
            userId,
            stolenToken,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddMinutes(-30)); // Revoked 30 mins ago!

        _mediator.Handlers[typeof(GetRefreshTokenQuery)] = Result<RefreshTokenDto>.Success(revokedTokenDto);

        // Act
        var result = await _handler.Handle(new RefreshTokenOrchestrator(stolenToken), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AUTH_TOKEN_THEFT_DETECTED", result.Errors.First().Code);

        // Verify that ALL refresh tokens in SQL Server were revoked for this user
        Assert.Contains(_mediator.SentCommands, cmd =>
            cmd is RevokeAllRefreshTokensCommand r && r.UserId == userId);

        // Verify that all active JWTs were invalidated in Redis
        var isCutoffRevoked = await _tokenRevocationStore.IsTokenRevokedAsync(null, userGuid, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1);
        Assert.True(isCutoffRevoked);
    }
}
