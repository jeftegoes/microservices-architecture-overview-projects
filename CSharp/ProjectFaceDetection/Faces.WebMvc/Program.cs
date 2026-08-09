using Faces.WebMvc;
using Faces.WebMvc.RestClients;
using Faces.WebMvc.Services;
using MassTransit;
using Messaging.Interfaces.SharedLib.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IHostedService, BusService>();

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddMassTransit(c =>
{
    c.UsingRabbitMq((context, config) =>
    {
        config.Host("rabbitmq", "/", h =>
        {
            h.Username(RabbitMqMassTransitConstants.UserName);
            h.Password(RabbitMqMassTransitConstants.Password);
        });
    });
});

builder.Services.AddHttpClient<IOrderManagementApi, OrderManagementApi>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
