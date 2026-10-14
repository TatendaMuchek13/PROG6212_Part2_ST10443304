using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("Events")]
    public class Event
    {
        
      [Key] public int EventID { get; set; }

        public int OrganiserCode { get; set; }
        [ForeignKey("OrganiserCode")]
        public Organiser? Organiser { get; set; }// references the Organiser table

        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime DateOfEvent { get; set; }
        public string RouteInfo { get; set; }      // This will be the location
        public double Distance { get; set; }       
        public string EventType { get; set; }      // Run, Walk or Cycle
    }
}