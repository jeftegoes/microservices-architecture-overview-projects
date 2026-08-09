using Amazon.DynamoDBv2.DataModel;

namespace Recruiter.LambdaConsole.Models
{
    [DynamoDBTable("Candidates")]
    public class Candidate
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
    }
}