using MassTransit;
using System.Reflection;

namespace UserProfileService.MessageBroker;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumers(Assembly.GetExecutingAssembly());

            x.UsingRabbitMq((context, cfg) =>
            {
                // Read from appsettings.json
                var host = configuration["RabbitMQ:Host"]!;
                var virtualHost = configuration["RabbitMQ:VirtualHost"]!;
                var username = configuration["RabbitMQ:Username"]!;
                var password = configuration["RabbitMQ:Password"]!;

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // This automatically configures all the endpoints based on your ConsumerDefinitions
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}