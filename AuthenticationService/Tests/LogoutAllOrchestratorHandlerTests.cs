using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;
using AuthenticationService.Features.Auth.Orchestrators.LogoutAll;
using Xunit;

namespace AuthenticationService.Tests;

public class LogoutAllOrchestratorHandlerTests
{
    private readonly FakeMediator _mediator;
    private readonly FakeTokenRevocationStore _tokenRevocationStore;
    private readonly LogoutAllOrchestratorHandler _handler;

    public LogoutAllOrchestratorHandlerTests()
    {
        _mediator = new FakeMediator();
        _tokenRevocationStore = new FakeTokenRevocationStore();
        _handler = new LogoutAllOrchestratorHandler(_mediator, _tokenRevocationStore);
    }

    [Fact]
    public async Task Handle_WhenValidUser_RevokesAllRefreshTokens_AndSetsUserCutoffInRedis()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var userId = userGuid.ToString();
        var request = new LogoutAllOrchestrator(userId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // 1. Verify SQL revocation was dispatched
        Assert.Contains(_mediator.SentCommands, cmd =>
            cmd is RevokeAllRefreshTokensCommand r && r.UserId == userId);

        // 2. Verify Redis user cutoff was set
        var isCutoffRevoked = await _tokenRevocationStore.IsTokenRevokedAsync(null, userGuid, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1);
        Assert.True(isCutoffRevoked);
    }
}
