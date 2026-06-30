namespace AuthenticationService.Models.Requests;

public record ChangePasswordRequest(string OldPassword, string NewPassword);