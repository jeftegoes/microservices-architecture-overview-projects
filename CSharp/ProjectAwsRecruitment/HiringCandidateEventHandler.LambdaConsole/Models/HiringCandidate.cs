using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("HiringCandidate")]
public class HiringCandidate
{
    [DynamoDBHashKey("userId")]
    public string UserId { get; set; }

    [DynamoDBRangeKey("Id")]
    public string Id { get; set; }

    public string Name { get; set; }
    public int Salary { get; set; }
    public int Rating { get; set; }
    public string CityName { get; set; }
    public string Photo { get; set; }
    public DateTime CreationDateTime { get; set; }
}