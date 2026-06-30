namespace AuthenticationService.Services;

public class GrpcIntegrationService : IGrpcIntegrationService
{
    public Task<bool> HasCompletedProfileAsync(string userId)
    {
        // TODO: Implement actual gRPC call to ProfileService
        // var client = new ProfileGrpcService.ProfileGrpcServiceClient(_channel);
        // var response = await client.HasCompletedProfileAsync(new ProfileRequest { UserId = userId });
        // return response.HasCompleted;

        return Task.FromResult(true); // Dummy data
    }

    public Task<bool> IsPremiumUserAsync(string userId)
    {
        // TODO: Implement actual gRPC call to SubscriptionService
        //var client = new SubscriptionGrpcService.SubscriptionGrpcServiceClient(_channel);
        // var response = await client.GetSubscriptionStatusAsync(new SubscriptionRequest { UserId = userId });
        // return response.IsPremium;

        return Task.FromResult(false); // Dummy data
    }
}