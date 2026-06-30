using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.Login;
using AuthenticationService.Features.Auth.Commands.Logout;
using AuthenticationService.Features.Auth.Commands.SendOtp;
using AuthenticationService.Features.Auth.Commands.VerifyOtp;
using AuthenticationService.Features.Auth.Orchestrators.ChangePassword;
using AuthenticationService.Features.Auth.Orchestrators.ForgotPassword;
using AuthenticationService.Features.Auth.Orchestrators.Login;
using AuthenticationService.Features.Auth.Orchestrators.RefreshToken;
using AuthenticationService.Features.Auth.Orchestrators.Register;
using AuthenticationService.Features.Auth.Orchestrators.ResetPassword;
using AuthenticationService.Models.Requests;
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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterOrchestrator(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isDuplicateEmail = result.Errors.Any(e =>
                e.Code == AuthErrorCodes.DuplicateUserName || e.Code == AuthErrorCodes.DuplicateEmail);

            var statusCode = isDuplicateEmail ? 409 : 400;
            var message = isDuplicateEmail ? "User already exists" : "Registration failed";

            return StatusCode(statusCode, ApiResponse<RegisterOrchestratorResponse>.Failure(
                result.Errors.Select(e => e.Description),
                message,
                statusCode));
        }

        return Created("",
            ApiResponse<RegisterOrchestratorResponse>.Success(result.Value, "User registered successfully", 201));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginOrchestrator command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isLocked = result.Errors.Any(e => e.Code == AuthErrorCodes.AccountLocked);
            var statusCode = isLocked ? 423 : 401;

            return StatusCode(statusCode, ApiResponse<LoginCommandResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                statusCode));
        }

        return Ok(ApiResponse<LoginCommandResponse>.Success(result.Value, "User successfully authenticated."));
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var command = new SendOtpCommand(request.Email);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isRateLimited = result.Errors.Any(e => e.Code == AuthErrorCodes.RateLimitExceeded);
            var statusCode = isRateLimited ? 429 : 400;

            return StatusCode(statusCode, ApiResponse<SendOtpCommandResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                statusCode));
        }

        return Ok(ApiResponse<SendOtpCommandResponse>.Success(result.Value, "OTP sent successfully."));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var command = new VerifyOtpCommand(request.Email, request.Otp);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return StatusCode(401, ApiResponse<VerifyOtpCommandResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description,
                401));

        return Ok(ApiResponse<VerifyOtpCommandResponse>.Success(result.Value, "OTP verified successfully."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordOrchestrator command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == AuthErrorCodes.UserNotFound);
            if (isNotFound)
                // To prevent email enumeration, we return 200 OK even if the user is not found.
                // In some systems, an explicit 404 is preferred. We will return 404 here to align with SendOtp logic.
                return StatusCode(404, ApiResponse<bool>.Failure(
                    result.Errors.Select(e => e.Description),
                    "User not found.",
                    404));

            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));
        }

        return Ok(ApiResponse<bool>.Success(result.Value, "Password reset OTP sent to your email."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordOrchestrator command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));

        return Ok(ApiResponse<bool>.Success(result.Value, "Password has been successfully reset."));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenOrchestrator command)
    {
        var result = await _mediator.Send(command);

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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? string.Empty;

        var command = new ChangePasswordOrchestrator(userId, request.OldPassword, request.NewPassword);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<bool>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));

        return Ok(ApiResponse<bool>.Success(result.Value, "Password changed successfully."));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? string.Empty;

        var command = new LogoutCommand(request.RefreshToken, userId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<Unit>.Failure(
                result.Errors.Select(e => e.Description),
                result.Errors.First().Description));

        return Ok(ApiResponse<Unit>.Success(result.Value, "Logged out successfully."));
    }
}