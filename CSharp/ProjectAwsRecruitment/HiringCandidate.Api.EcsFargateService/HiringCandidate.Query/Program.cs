using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(x =>
    x.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

string GetRegionName() =>
    Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";

IEnumerable<Claim> GetTokenClaims(string token)
{
    var details = new JwtSecurityToken(token);
    return details.Claims;
}

app.MapGet("/query", async (string? idToken) =>
{
    var userId = GetTokenClaims(idToken).FirstOrDefault(x => x.Type == "cognito:username")?.Value ?? "";
    var groups = GetTokenClaims(idToken).FirstOrDefault(x => x.Type == "cognito:groups")?.Value ?? "";

    var hiredDto = new List<HiredDto>();

    var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));

    using (var dbContext = new DynamoDBContext(dbClient))
    {
        if (string.IsNullOrEmpty(groups))
        {
            hiredDto.AddRange(await dbContext.FromQueryAsync<HiredDto>(new QueryOperationConfig()
            {
                Filter = new QueryFilter("userId", QueryOperator.Equal, userId),
                IndexName = "userId-index"
            }).GetRemainingAsync());
        }
        else
        {
            if (groups.Contains("AdminGroup"))
            {
                hiredDto.AddRange(await dbContext.QueryAsync<HiredDto>(1, new DynamoDBOperationConfig()
                {
                    IndexName = "HiringStatus-index"
                }).GetRemainingAsync());
            }
        }
    };

    return hiredDto;
});

app.MapGet("/health", () => new HttpResponseMessage(HttpStatusCode.OK));

app.Run();
