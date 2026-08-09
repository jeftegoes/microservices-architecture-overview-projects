namespace Recruiter.LambdaConsole.Models
{
    public class CandidateCreatedEvent
    {
        public string UserId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Salary { get; set; }
        public int Rating { get; set; }
        public string CityName { get; set; }
        public string Photo { get; set; }

        public DateTime CreationDateTime { get; set; }
    }
}