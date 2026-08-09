using System;
using GreenPipes;
using MassTransit;
using Messaging.Interfaces.SharedLib.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Orders.Api.Hubs;
using Orders.Api.Messages.Consumers;
using Orders.Api.Persistence;

namespace Orders.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<OrderSettings>(Configuration);
            services.AddDbContext<OrderContext>(options => options.UseSqlServer
            (
                Configuration["OrderContextConnection"]
            ));
            services.AddHttpClient();
            services.AddSignalR();

            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddMassTransit(c =>
            {
                c.AddConsumer<RegisterOrderCommandConsumer>();
                c.AddConsumer<OrderDispatchedEventConsumer>();

                c.UsingRabbitMq((context, config) =>
                {
                    config.Host("rabbitmq", "/", h =>
                    {
                        h.Username(RabbitMqMassTransitConstants.UserName);
                        h.Password(RabbitMqMassTransitConstants.Password);
                    });

                    config.ReceiveEndpoint(RabbitMqMassTransitConstants.RegisterOrderCommandQueue, e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                        e.Consumer<RegisterOrderCommandConsumer>(context);
                    });

                    config.ReceiveEndpoint(RabbitMqMassTransitConstants.OrderDispatchedServiceQueue, e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(x => x.Interval(2, 100));
                        e.Consumer<OrderDispatchedEventConsumer>(context);
                    });
                });
            });

            services.AddMassTransitHostedService();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials());
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<OrderHub>("/ordersHub");
            });

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<OrderContext>().MigrateDb();
        }
    }
}
