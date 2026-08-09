using Email.Service;
using MassTransit;
using Messaging.Interfaces.SharedLib.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Service;
using Notification.Service.Consumers;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile($"appsettings.json", optional: false);
                configHost.AddEnvironmentVariables();
                configHost.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                var environmentName = $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json";
                config.AddJsonFile(environmentName, optional: false);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var emailConfig = hostContext.Configuration
                    .GetSection("EmailConfiguration")
                    .Get<EmailConfig>();

                services.AddSingleton(emailConfig);
                services.AddScoped<IEmailSender, EmailSender>();

                services.AddMassTransit(c =>
                {
                    c.AddConsumer<OrderProcessedEventConsumer>();

                    c.UsingRabbitMq((context, config) =>
                    {
                        config.Host("rabbitmq", "/", h =>
                        {
                            h.Username(RabbitMqMassTransitConstants.UserName);
                            h.Password(RabbitMqMassTransitConstants.Password);
                        });

                        config.ReceiveEndpoint(RabbitMqMassTransitConstants.NotificationServiceQueue, e =>
                        {
                            e.PrefetchCount = 16;
                            e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                            e.Consumer<OrderProcessedEventConsumer>(context);
                        });
                    });
                });

                services.AddSingleton<IHostedService, BusService>();
            });

        return hostBuilder;
    }
}