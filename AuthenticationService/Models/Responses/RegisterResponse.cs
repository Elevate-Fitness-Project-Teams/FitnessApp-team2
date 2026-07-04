namespace AuthenticationService.Models.Responses;

/// <summary>
/// API response DTO for the register endpoint.
/// Kept in Models/Responses because it is an API contract concern, not a command concern.
/// </summary>
public record RegisterResponse(string UserId, bool RequiresProfileCompletion);
