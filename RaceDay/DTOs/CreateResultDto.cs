using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The enrolment a result is for, plus the result details.</summary>
    public class CreateResultDto : UpdateResultDto
    {
        /// <summary>The id of the enrolment the result belongs to. The event must belong to the logged-in Organiser.</summary>
        [Range(1, int.MaxValue)]
        public int EnrollID { get; set; }
    }
}