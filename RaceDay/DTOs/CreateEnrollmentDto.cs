using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class CreateEnrollmentDto
    {
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Range(1, int.MaxValue)]
        public int CateID { get; set; }
    }
}