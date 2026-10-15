using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class EventDto
    {
        [Required]
        public string EventName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime DateOfEvent { get; set; }

        [Required]
        public string RouteInfo { get; set; }     
        [Range(0.1, 1000)]
        public double Distance { get; set; }      

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
        public string EventType { get; set; }
    }
}