namespace RaceDayApi.DTOs
{
    public class ResultDto
    {
        public int ResultsID { get; set; }
        public int EnrollID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int CateID { get; set; }
        public string CategoryName { get; set; }
        public int PNumber { get; set; }
        public string ParticipantName { get; set; }
        public string ResultsStatus { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }
}