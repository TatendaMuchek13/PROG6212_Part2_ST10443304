namespace RaceDayApi.DTOs
{
    public class EnrollmentDto
    {
        public int EnrollID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int CateID { get; set; }
        public string CategoryName { get; set; }
        public int PNumber { get; set; }
        public string ParticipantName { get; set; }
        public DateTime DateOfEnroll { get; set; }
    }
}