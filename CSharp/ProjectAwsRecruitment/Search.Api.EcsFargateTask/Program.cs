using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Nest;
using Polly;
using Polly.CircuitBreaker;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var circuitBreakerPolicy = Policy<List<Candidate>>
    .Handle<Exception>()
    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

app.MapGet("/search", async (string? city, int? rating) =>
{
    var result = new HttpResponseMessage();

    try
    {
        var candidates = circuitBreakerPolicy.ExecuteAsync(async () =>
        {
            return await SearchCandidates(city, rating);
        });

        result.StatusCode = HttpStatusCode.OK;
        result.Content = new StringContent(JsonSerializer.Serialize(candidates));
        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return result;
    }
    catch (BrokenCircuitException)
    {
        result.StatusCode = HttpStatusCode.NotAcceptable;
        result.ReasonPhrase = "Circuit is OPEN.";
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        throw;
    }

    return result;
});

async Task<List<Candidate>> SearchCandidates(string? city, int? rating)
{
    var host = Environment.GetEnvironmentVariable("host");
    var username = Environment.GetEnvironmentVariable("userName");
    var password = Environment.GetEnvironmentVariable("password");
    var indexName = Environment.GetEnvironmentVariable("indexName");

    var connSettings = new ConnectionSettings(new Uri(host));
    connSettings.BasicAuthentication(username, password);
    connSettings.DefaultIndex(indexName);
    connSettings.DefaultMappingFor<Candidate>(n => n.IdProperty(p => p.Id));

    var esClient = new ElasticClient(connSettings);

    if (rating is null)
        rating = 1;

    ISearchResponse<Candidate> result;

    if (city is null)
        result = await esClient.SearchAsync<Candidate>(
            s => s.Query(
                q => q.MatchAll()
                &&
                q.Range(
                    p => p.Field(f => f.Rating).GreaterThanOrEquals(rating)
                )
            )
        );

    result = await esClient.SearchAsync<Candidate>(
        s => s.Query(
            q => q.Prefix(
                p => p.Field(f => f.CityName).Value(city).CaseInsensitive()
            )
            &&
            q.Range(
                p => p.Field(f => f.Rating).GreaterThanOrEquals(rating)
            )
        )
    );

    return result.Hits.Select(x => x.Source).ToList();
}

app.Run();