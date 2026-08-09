using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;
using Nest;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class CandidateCreatedEvent
{
    private string GetRegionName()
    {
        return Environment.GetEnvironmentVariable("AWS_REGION") ?? "sa-east-1";
    }

    public async Task Handler(SNSEvent snsEvent)
    {
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(GetRegionName()));
        var table = Table.LoadTable(dbClient, "CandidatesCreatedEventIds");

        var host = Environment.GetEnvironmentVariable("host");
        var username = Environment.GetEnvironmentVariable("userName");
        var password = Environment.GetEnvironmentVariable("password");
        var indexName = Environment.GetEnvironmentVariable("indexName");

        var connSettings = new ConnectionSettings(new Uri(host));
        connSettings.BasicAuthentication(username, password);
        connSettings.DefaultIndex(indexName);
        connSettings.DefaultMappingFor<Candidate>(n => n.IdProperty(p => p.Id));

        var esClient = new ElasticClient(connSettings);

        if (!(await esClient.Indices.ExistsAsync(indexName)).Exists)
        {
            await esClient.Indices.CreateAsync(indexName);
        }

        foreach (var eventRecord in snsEvent.Records)
        {
            var eventId = eventRecord.Sns.MessageId;
            var foundItem = await table.GetItemAsync(eventId);

            if (foundItem == null)
            {
                await table.PutItemAsync(new Document()
                {
                    ["eventId"] = eventId
                });
            }

            var candidate = JsonSerializer.Deserialize<Candidate>(eventRecord.Sns.Message);
            await esClient.IndexDocumentAsync(candidate);
        }
    }
}