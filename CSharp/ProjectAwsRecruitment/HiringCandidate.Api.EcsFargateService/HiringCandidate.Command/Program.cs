using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(x =>
    x.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyHeader()));

var app = builder.Build();

string GetRegionName() =>
    Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";

IEnumerable<Claim> GetTokenClaims(string token)
{
    var details = new JwtSecurityToken(token);
    return details.Claims;
}

app.MapPost("/hiring", async ([FromBody] HiredRequest request) =>
{
    var userId = GetTokenClaims(request.IdToken).FirstOrDefault(x => x.Type == "cognito:username")?.Value ?? "";

    var hired = new Hired()
    {
        CandidateId = request.CandidateId,
        HiringDate = request.HiringDate,
        StartDate = request.StartDate,
        UserId = userId,
        Id = Guid.NewGuid().ToString(),
        LinkedIn = string.Empty,
        PhoneNumber = string.Empty,
        Email = string.Empty,
        HiringStatus = HiringStatus.Pending
    };

    var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));

    using (var dbContext = new DynamoDBContext(dbClient))
    {
        await dbContext.SaveAsync(hired);
    };
});

app.MapGet("/health", () => new HttpResponseMessage(HttpStatusCode.OK));

app.Run();