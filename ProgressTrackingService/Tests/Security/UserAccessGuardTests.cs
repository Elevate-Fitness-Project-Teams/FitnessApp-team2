using System.Security.Claims;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Common.Security;
using Xunit;

namespace ProgressTrackingService.Tests.Security;

public class UserAccessGuardTests
{
    [Fact]
    public void CanAccess_WhenCallerIsTargetUser_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var result = UserAccessGuard.CanAccess(principal, userId);

        Assert.True(result);
    }

    [Fact]
    public void CanAccess_WhenCallerIsDifferentUser_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var result = UserAccessGuard.CanAccess(principal, otherUserId);

        Assert.False(result);
    }

    [Fact]
    public void CanAccess_WhenCallerIsAdmin_ReturnsTrueForAnyUser()
    {
        var adminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var result = UserAccessGuard.CanAccess(principal, targetUserId);

        Assert.True(result);
    }

    [Fact]
    public void CanAccess_WhenUnauthenticated_ReturnsFalse()
    {
        var targetUserId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = UserAccessGuard.CanAccess(principal, targetUserId);

        Assert.False(result);
    }

    [Fact]
    public void ValidateAccess_WhenCallerIsDifferentUser_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("sub", userId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var result = UserAccessGuard.ValidateAccess(principal, targetUserId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.PermForbidden, result.Error.Code);
    }

    [Fact]
    public void ValidateAccess_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = UserAccessGuard.ValidateAccess(principal, targetUserId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.AuthTokenInvalid, result.Error.Code);
    }

    [Fact]
    public void GetUserId_SupportsSubAndSidClaims()
    {
        var userId = Guid.NewGuid();
        var subPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", userId.ToString()) }, "Auth"));
        var sidPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Sid, userId.ToString()) }, "Auth"));

        Assert.Equal(userId, subPrincipal.GetUserId());
        Assert.Equal(userId, sidPrincipal.GetUserId());
    }
}
