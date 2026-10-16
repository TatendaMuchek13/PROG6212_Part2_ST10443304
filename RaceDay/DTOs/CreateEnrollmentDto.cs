using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The event and category a participant wants to enter.</summary>
    public class CreateEnrollmentDto
    {
        /// <summary>The id of the event to enter.</summary>
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        /// <summary>The id of the category to enter. It must belong to the chosen event.</summary>
        [Range(1, int.MaxValue)]
        public int CateID { get; set; }
    }
}