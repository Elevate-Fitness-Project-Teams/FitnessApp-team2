using System.Net;
using System.Security.Claims;
using ApiGateway.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace ApiGateway.Tests;

public class TokenRevocationMiddlewareTests
{
    private readonly ServiceCollection _services;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public TokenRevocationMiddlewareTests()
    {
        _services = new ServiceCollection();
        _redis = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();

        _redis.IsConnected.Returns(true);
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_database);

        _services.AddSingleton<IConnectionMultiplexer>(_redis);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthenticated_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TokenRevocationMiddleware(
            next,
            _services.BuildServiceProvider(),
            NullLogger<TokenRevocationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenTokenJtiIsRevoked_Returns401AndDoesNotCallNext()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // MGET returns [ "1", nil ]
        _database.StringGetAsync(Arg.Any<RedisKey[]>())
            .Returns(new RedisValue[] { "1", RedisValue.Null });

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("jti", jti),
            new Claim("sub", userId),
            new Claim("iat", iat)
        }, "Bearer");
        context.User = new ClaimsPrincipal(identity);
        context.Response.Body = new MemoryStream();

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TokenRevocationMiddleware(
            next,
            _services.BuildServiceProvider(),
            NullLogger<TokenRevocationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserRevokedBeforeCutoffExceedsIssuedAt_Returns401()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var iat = (nowUnix - 50).ToString(); // Issued 50 seconds ago

        // MGET returns [ nil, (nowUnix - 10) ] -> Cutoff is newer than iat -> Revoked!
        _database.StringGetAsync(Arg.Any<RedisKey[]>())
            .Returns(new RedisValue[] { RedisValue.Null, (nowUnix - 10).ToString() });

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("jti", jti),
            new Claim("sub", userId),
            new Claim("iat", iat)
        }, "Bearer");
        context.User = new ClaimsPrincipal(identity);
        context.Response.Body = new MemoryStream();

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TokenRevocationMiddleware(
            next,
            _services.BuildServiceProvider(),
            NullLogger<TokenRevocationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenTokenIsCleanAndIssuedAfterCutoff_CallsNext()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var iat = (nowUnix + 5).ToString(); // Newly minted token after cutoff

        // MGET returns [ nil, nowUnix ] -> Cutoff is older than iat -> Clean!
        _database.StringGetAsync(Arg.Any<RedisKey[]>())
            .Returns(new RedisValue[] { RedisValue.Null, nowUnix.ToString() });

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("jti", jti),
            new Claim("sub", userId),
            new Claim("iat", iat)
        }, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TokenRevocationMiddleware(
            next,
            _services.BuildServiceProvider(),
            NullLogger<TokenRevocationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenRedisIsDisconnected_FailsOpenAndCallsNext()
    {
        // Arrange
        _redis.IsConnected.Returns(false);

        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("jti", Guid.NewGuid().ToString()),
            new Claim("sub", Guid.NewGuid().ToString())
        }, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TokenRevocationMiddleware(
            next,
            _services.BuildServiceProvider(),
            NullLogger<TokenRevocationMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
