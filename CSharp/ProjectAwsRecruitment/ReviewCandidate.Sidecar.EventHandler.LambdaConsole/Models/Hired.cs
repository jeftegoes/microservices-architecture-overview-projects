using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Hired")]
public class Hired
{
    public string? HiredId { get; set; }
    public int Status { get; set; }
}