using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using AuthenticationService.Features.Auth.Orchestrators.Logout;
using AuthenticationService.Services;
using MediatR;
using Xunit;

namespace AuthenticationService.Tests;

public class LogoutOrchestratorHandlerTests
{
    private readonly FakeMediator _mediator;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeTokenRevocationStore _tokenRevocationStore;
    private readonly LogoutOrchestratorHandler _handler;

    public LogoutOrchestratorHandlerTests()
    {
        _mediator = new FakeMediator();
        _unitOfWork = new FakeUnitOfWork();
        _tokenRevocationStore = new FakeTokenRevocationStore();
        _handler = new LogoutOrchestratorHandler(_mediator, _unitOfWork, _tokenRevocationStore);
    }

    [Fact]
    public async Task Handle_WhenValidLogoutRequest_RevokesRefreshTokenAndBlacklistsJti()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var jti = Guid.NewGuid().ToString();
        var refreshToken = "valid-refresh-token-xyz";
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var request = new LogoutOrchestrator(refreshToken, userId, jti, expiresAt);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify Refresh Token was revoked with userId
        Assert.Contains(_mediator.SentCommands, cmd =>
            cmd is RevokeRefreshTokenCommand r && r.Token == refreshToken && r.UserId == userId);

        // Verify JWT JTI was blacklisted in Redis store
        var isJtiRevoked = await _tokenRevocationStore.IsTokenRevokedAsync(jti);
        Assert.True(isJtiRevoked);
    }

    [Fact]
    public async Task Handle_WhenJtiMissing_StillRevokesRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var refreshToken = "valid-refresh-token-xyz";
        var request = new LogoutOrchestrator(refreshToken, userId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(_mediator.SentCommands, cmd =>
            cmd is RevokeRefreshTokenCommand r && r.Token == refreshToken && r.UserId == userId);
    }
}

public class FakeUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await action();
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        return await action();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class FakeMediator : IMediator
{
    public List<object> SentCommands { get; } = new();
    public Dictionary<Type, object> Handlers { get; } = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        SentCommands.Add(request);
        if (Handlers.TryGetValue(request.GetType(), out var response))
        {
            return Task.FromResult((TResponse)response);
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return Task.FromResult((TResponse)(object)Result.Success());
        }

        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        SentCommands.Add(request);
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        SentCommands.Add(request);
        return Task.FromResult<object?>(null);
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
