using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Orchestrators.ChangePassword;
using AuthenticationService.Features.Auth.Orchestrators.ForgotPassword;
using AuthenticationService.Features.Auth.Orchestrators.Login;
using AuthenticationService.Features.Auth.Orchestrators.Logout;
using AuthenticationService.Features.Auth.Orchestrators.RefreshToken;
using AuthenticationService.Features.Auth.Orchestrators.Register;
using AuthenticationService.Features.Auth.Orchestrators.ResetPassword;
using AuthenticationService.Features.Auth.Orchestrators.SendOtp;
using AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;
using AuthenticationService.Models.Requests;
using AuthenticationService.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterOrchestrator(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var isDuplicateEmail = result.Errors.Any(e => e.Code == AuthErrorCodes.DuplicateEmail);

            var statusCode = isDuplicateEmail ? 409 : 400;
            var message = isDuplicateEmail ? "User already exists" : "Registration failed";

            return StatusCode(statusCode, ApiResponse<RegisterResponse>.Failure(
                result.Errors.Select(e => e.Description),
                message,
                statusCode));
        }

        return CreatedAtAction(nameof(Register), new { id = result.Value.UserId },
            ApiResponse<RegisterResponse>.Success(result.Value, "User registered successfully", 201));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginOrchestrator command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isLocked = result.Errors.Any(e => e.Code == AuthErrorCodes.AccountLocked);
            var statusCode = isLocked ? 423 : 401;

            return StatusCode(statusCode, ApiResponse<LoginResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                statusCode));
        }

        return Ok(ApiResponse<LoginResponse>.Success(result.Value, "User successfully authenticated."));
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        var command = new SendOtpOrchestrator(request.Email);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var isRateLimited = result.Errors.Any(e => e.Code == AuthErrorCodes.RateLimitExceeded);
            var statusCode = isRateLimited ? 429 : 400;

            return StatusCode(statusCode, ApiResponse<SendOtpResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                statusCode));
        }

        return Ok(ApiResponse<SendOtpResponse>.Success(result.Value, "OTP sent successfully."));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyOtpOrchestrator(request.Email, request.Otp);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return StatusCode(401, ApiResponse<VerifyOtpResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                401));

        return Ok(ApiResponse<VerifyOtpResponse>.Success(result.Value, "OTP verified successfully."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordOrchestrator command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));
        }

        return Ok(ApiResponse<bool>.Success(result.Value, "Password reset OTP sent to your email."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordOrchestrator command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));

        return Ok(ApiResponse<bool>.Success(result.Value, "Password has been successfully reset."));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenOrchestrator command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return StatusCode(401, ApiResponse<RefreshTokenOrchestratorResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                401));

        return Ok(ApiResponse<RefreshTokenOrchestratorResponse>.Success(result.Value,
            "Tokens refreshed successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? string.Empty;

        var command = new ChangePasswordOrchestrator(userId, request.OldPassword, request.NewPassword);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));

        return Ok(ApiResponse<bool>.Success(result.Value, "Password changed successfully."));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new LogoutOrchestrator(request.RefreshToken, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));
        }

        return Ok(ApiResponse<bool>.Success(true, "Logged out successfully."));
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(ApiResponse<bool>.Failure(
                new List<string> { "Token is required" },
                "Invalid request",
                400));
        }

        var command = new Features.Auth.Commands.ValidateToken.ValidateTokenCommand(request.Token);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(401, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                401));
        }

        return Ok(ApiResponse<bool>.Success(true, "Token is valid."));
    }
}