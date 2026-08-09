using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Hired")]
public class HiredDto
{
    [DynamoDBHashKey("userId")]
    public string? UserId { get; set; }
    [DynamoDBRangeKey("Id")]
    public string? Id { get; set; }

    public string? IdToken { get; set; }
    public string? CandidateId { get; set; }
    public string? HiringDate { get; set; }
    public string? StartDate { get; set; }
    public string? LinkedIn { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int HiringStatus { get; set; }
}