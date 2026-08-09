using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class ReviewCandidateEvent
{
    private string GetRegionName()
    {
        return Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
    }

    public async Task Handler(SNSEvent snsEvent, ILambdaContext context)
    {
        Console.WriteLine(JsonSerializer.Serialize(snsEvent));
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
        var table = Table.LoadTable(dbClient, "Hired");

        foreach (var eventRecord in snsEvent.Records)
        {
            var hired = JsonSerializer.Deserialize<Hired>(eventRecord.Sns.Message);

            if (hired == null)
            {
                context.Logger.LogError("SNS don't work...");
                continue;
            }
            
            Console.WriteLine("HiredId HERE!!!: {0}", hired.HiredId);

            var queryResult = await table.Query(new QueryOperationConfig
            {
                Filter = new QueryFilter("Id", QueryOperator.Equal, hired.HiredId),
                IndexName = "Id-index"
            }).GetRemainingAsync();

            var document = queryResult.FirstOrDefault();

            Console.WriteLine(JsonSerializer.Serialize(document));

            if (document != null)
            {
                document["HiringStatus"] = hired.Status;
                await table.UpdateItemAsync(document, document["userId"].ToString(), hired.HiredId);
            }
            else
            {
                context.Logger.LogError($"Candidate {hired.HiredId} was not found.");
            }
        }
    }
}
