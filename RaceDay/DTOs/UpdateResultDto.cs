using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class UpdateResultDto
    {
        [Required]
        [RegularExpression("^(Finished|DNF|DNS)$", ErrorMessage = "ResultsStatus must be Finished, DNF or DNS.")]
        public string ResultsStatus { get; set; }

        public TimeSpan FinishTime { get; set; }      //  "00:42:35" Format

        [Range(0, 100000)]
        public int Position { get; set; }             // 0 means no position yet
    }
}