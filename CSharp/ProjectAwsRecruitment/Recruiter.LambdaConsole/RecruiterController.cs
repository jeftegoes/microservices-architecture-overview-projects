using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AutoMapper;
using HttpMultipartParser;
using Recruiter.LambdaConsole.Models;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class RecruiterController
{
    private APIGatewayProxyResponse GetDefaultResponse()
    {
        var response = new APIGatewayProxyResponse()
        {
            Headers = new Dictionary<string, string>(),
            StatusCode = 200
        };

        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Headers", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST");
        response.Headers.Add("Content-Type", "application/json");

        return response;
    }

    private IEnumerable<Claim> GetTokenClaims(string token)
    {
        var details = new JwtSecurityToken(token);
        return details.Claims;
    }

    private string GetRegionName() =>
        Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";

    public async Task<APIGatewayProxyResponse> ListCandidates(APIGatewayProxyRequest request)
    {
        var response = GetDefaultResponse();

        if (request?.QueryStringParameters == null)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Body = "Query string is null.";
            return response;
        }

        var token = request.QueryStringParameters.ContainsKey("token") ? request.QueryStringParameters["token"] : "";
        if (string.IsNullOrEmpty(token))
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Body = JsonSerializer.Serialize(new { Error = "Query parameter 'token' not present." });
            return response;
        }

        var userId = GetTokenClaims(token).FirstOrDefault(x => x.Type == "cognito:username")?.Value;

        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));

        using (var dbContext = new DynamoDBContext(dbClient))
        {
            var condition = new[] { new ScanCondition("UserId", ScanOperator.Equal, userId) };
            var candidates = await dbContext.ScanAsync<Candidate>(condition).GetRemainingAsync();
            response.Body = JsonSerializer.Serialize(candidates);
        };

        return response;
    }

    public async Task<APIGatewayProxyResponse> SaveCandidate(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var response = GetDefaultResponse();

        var bodyContent = request.IsBase64Encoded ? Convert.FromBase64String(request.Body) : Encoding.UTF8.GetBytes(request.Body);

        using (var stream = new MemoryStream(bodyContent))
        {
            var formData = await MultipartFormDataParser.ParseAsync(stream).ConfigureAwait(false);

            var name = formData.GetParameterValue("name");
            var rating = formData.GetParameterValue("rating");
            var city = formData.GetParameterValue("city");
            var salary = formData.GetParameterValue("salary");

            var file = formData.Files.FirstOrDefault();
            var photo = file.FileName;

            var userId = formData.GetParameterValue("userId");
            var token = formData.GetParameterValue("idToken");

            var group = GetTokenClaims(token).FirstOrDefault(x => x.Type == "cognito:groups");

            if (group == null || group.Value != "AdminGroup")
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Body = JsonSerializer.Serialize(new { Message = "Must be administration group!" });
                return response;
            }

            var bucketName = Environment.GetEnvironmentVariable("bucketName");

            var s3client = new AmazonS3Client(RegionEndpoint.GetBySystemName(GetRegionName()));
            var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));

            await s3client.PutObjectAsync(new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = photo,
                InputStream = file.Data
            });

            var candidate = new Candidate()
            {
                UserId = userId,
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Salary = int.Parse(salary),
                Rating = int.Parse(rating),
                CityName = city,
                Photo = photo
            };

            using (var dbContext = new DynamoDBContext(dbClient))
            {
                await dbContext.SaveAsync(candidate);
            };

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Candidate, CandidateCreatedEvent>()
                    .ForMember(dest => dest.CreationDateTime, opt => opt.MapFrom(src => DateTime.Now))
                    .ReverseMap();
            });

            var mapper = new Mapper(mapperConfig);

            var candidateCreatedEvent = mapper.Map<CandidateCreatedEvent>(candidate);

            var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(GetRegionName()));

            var message = JsonSerializer.Serialize(candidateCreatedEvent);

            var publisgReponse = await snsClient.PublishAsync(new PublishRequest()
            {
                TopicArn = Environment.GetEnvironmentVariable("snsTopicArn"),
                Message = message
            });
        }

        response.Body = JsonSerializer.Serialize(new { Message = "Candidate saved successfully!" });

        return response;
    }
}