using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.SimpleNotificationService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(x =>
    x.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

string GetRegionName() =>
    Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";

app.MapPost("/approve", async (string? hiredId, int status) =>
{
    var topicArn = Environment.GetEnvironmentVariable("snsTopicArn");
    var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(GetRegionName()));
    await snsClient.PublishAsync(topicArn, JsonSerializer.Serialize(new ApprovalEvent()
    {
        HiredId = hiredId,
        Status = status
    }));
});

app.MapGet("/health", () => new HttpResponseMessage(HttpStatusCode.OK));

app.Run();