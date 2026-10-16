using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The details of an event, used when creating or updating one.</summary>
    public class EventDto
    {
        /// <summary>The name of the event, for example Joburg Sprints.</summary>
        [Required]
        public string EventName { get; set; }

        /// <summary>A short description of the event.</summary>
        [Required]
        public string Description { get; set; }

        /// <summary>The date and time the event starts, for example 2026-12-05T07:00:00.</summary>
        [Required]
        public DateTime DateOfEvent { get; set; }

        /// <summary>The location or route of the event, for example Joburg City Centre.</summary>
        [Required]
        public string RouteInfo { get; set; }

        /// <summary>The distance in kilometres, between 0.1 and 1000.</summary>
        [Range(0.1, 1000)]
        public double Distance { get; set; }

        /// <summary>The type of event. Must be exactly Run, Walk or Cycle.</summary>
        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
        public string EventType { get; set; }
    }
}