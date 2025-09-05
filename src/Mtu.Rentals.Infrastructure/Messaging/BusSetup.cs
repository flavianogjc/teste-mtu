using MassTransit;
using Mtu.Rentals.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public static class BusSetup
{
    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<MotorcycleRegisteredConsumer>();

            x.UsingRabbitMq((ctx, bus) =>
            {
                var host = cfg["RabbitMQ:Host"] ?? "rabbitmq";
                var vhost = cfg["RabbitMQ:VirtualHost"] ?? "/"; // se você usar
                var user = cfg["RabbitMQ:User"] ?? "guest";
                var pass = cfg["RabbitMQ:Pass"] ?? "guest";

                bus.Host(host, vhost, h =>
                {
                    h.Username(user);
                    h.Password(pass);
                });

                bus.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
