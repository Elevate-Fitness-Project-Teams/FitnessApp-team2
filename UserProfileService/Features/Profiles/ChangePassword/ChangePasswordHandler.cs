using System.Net.Http.Json;
using MediatR;
using Microsoft.Extensions.Options;
using UserProfileService.Common;

namespace UserProfileService.Features.Profiles.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly HttpClient _httpClient;
    private readonly ServiceUrls _serviceUrls;

    public ChangePasswordHandler(HttpClient httpClient, IOptions<ServiceUrls> serviceUrls)
    {
        _httpClient = httpClient;
        _serviceUrls = serviceUrls.Value;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Build the payload to send to the Auth Service
        var payload = new
        {
            userId = request.UserId,
            currentPassword = request.CurrentPassword,
            newPassword = request.NewPassword
        };

        // Build the full URL: {BaseUrl}/api/v1/auth/internal/change-password
        var url = $"{_serviceUrls.AuthenticationService.TrimEnd('/')}/api/v1/auth/internal/change-password";

        // Call the Auth Service via HTTP POST
        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

        // If the Auth Service returned an error (wrong current password, user not found, etc.)
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            return Result.Failure(Error.Failure(
                "PasswordChangeFailed",
                $"Auth Service rejected the password change: {errorBody}"));
        }

        return Result.Success();
    }
}
