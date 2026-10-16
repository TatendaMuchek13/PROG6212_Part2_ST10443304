using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The result details for an enrolment.</summary>
    public class UpdateResultDto
    {
        /// <summary>The outcome. Must be exactly Finished, DNF (did not finish) or DNS (did not start).</summary>
        [Required]
        [RegularExpression("^(Finished|DNF|DNS)$", ErrorMessage = "ResultsStatus must be Finished, DNF or DNS.")]
        public string ResultsStatus { get; set; }

        /// <summary>The finishing time as hours:minutes:seconds, for example 00:42:35.</summary>
        public TimeSpan FinishTime { get; set; }

        /// <summary>The finishing position. Use 1 or higher for a finisher, and 0 for DNF or DNS.</summary>
        [Range(0, 100000)]
        public int Position { get; set; }
    }
}