using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.IdentityModel.Tokens;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class Authorizer
{
    public async Task<APIGatewayCustomAuthorizerResponse> Auth(APIGatewayCustomAuthorizerRequest request)
    {
        var idToken = request.QueryStringParameters["token"];
        var idTokenDetails = new JwtSecurityToken(idToken);

        var kid = idTokenDetails.Header["kid"].ToString();
        var issuer = idTokenDetails.Claims.First(x => x.Type == "iss").Value;
        var audiance = idTokenDetails.Claims.First(x => x.Type == "aud").Value;
        var userId = idTokenDetails.Claims.First(x => x.Type == "sub").Value;

        var response = new APIGatewayCustomAuthorizerResponse()
        {
            PrincipalID = userId,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement()
                    {
                        Action = new HashSet<string>(){ "execute-api:Invoke" },
                        Effect = "Allow",
                        Resource = new HashSet<string>() { request.MethodArn }
                    }
                }
            }
        };

        var secretClient = new AmazonSecretsManagerClient();
        var secret = await secretClient.GetSecretValueAsync(new GetSecretValueRequest()
        {
            SecretId = "RecruitmentCognitoKey"
        });

        var privateKeys = secret.SecretString;

        var jwks = new JsonWebKeySet(privateKeys);

        var privateKey = jwks.Keys.First(x => x.Kid == kid);

        var handler = new JwtSecurityTokenHandler();
        var result = await handler.ValidateTokenAsync(idToken, new TokenValidationParameters()
        {
            ValidIssuer = issuer,
            ValidAudience = audiance,
            IssuerSigningKey = privateKey
        });

        if (!result.IsValid)
        {
            throw new UnauthorizedAccessException("Token not valid.");
        }

        var apiGroupMapping = new Dictionary<string, string>()
        {
            // {"listRecruiters", "Recruiter"},
            // {"recruiter", "Recruiter"}
            // {"listRecruiters", "AdminGroup"},
            {"recruiter", "AdminGroup"}
        };

        var expectedGroup = apiGroupMapping.FirstOrDefault(x => request.Path.Contains(x.Key, StringComparison.InvariantCultureIgnoreCase));

        if (!expectedGroup.Equals(default(KeyValuePair<string, string>)))
        {
            var userGroup = idTokenDetails.Claims.First(x => x.Type == "cognito:groups").Value;
            if (string.Compare(userGroup, expectedGroup.Value, StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                response.PolicyDocument.Statement[0].Effect = "Deny";
            }
        }

        return response;
    }
}