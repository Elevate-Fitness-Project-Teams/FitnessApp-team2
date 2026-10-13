using System.Security.Claims;

namespace FitnessCalculationService.Common.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInternalService { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var idClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User?.FindFirstValue("sub")
                          ?? User?.FindFirstValue("userId");

            if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var guid))
            {
                return guid;
            }

            return null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");

    public bool IsInternalService
    {
        get
        {
            var role = User?.FindFirstValue(ClaimTypes.Role) ?? User?.FindFirstValue("role");
            return string.Equals(role, "internal-service", StringComparison.OrdinalIgnoreCase)
                   || User?.HasClaim("scope", "internal") == true;
        }
    }
}
