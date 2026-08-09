using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class HiringCandidateEvent
{
    private string GetRegionName()
    {
        return Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
    }

    public async Task Handler(SNSEvent snsEvent)
    {
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
        var table = Table.LoadTable(dbClient, "CandidatesCreatedEventIds");

        foreach (var eventRecord in snsEvent.Records)
        {
            var eventId = eventRecord.Sns.MessageId;
            var foundItem = await table.GetItemAsync(eventId);
            
            var HiringCandidate = JsonSerializer.Deserialize<HiringCandidate>(eventRecord.Sns.Message);
            HiringCandidate.CreationDateTime = DateTime.Now;

            using (var dbContext = new DynamoDBContext(dbClient))
            {
                await dbContext.SaveAsync(HiringCandidate);
            };
        }
    }
}