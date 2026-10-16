using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    /// <summary>An age or distance category within an event, for example Under 20 or 10km.</summary>
    public class Category
    {
        /// <summary>The id of the category.</summary>
        [Key]
        public int CateID { get; set; }

        /// <summary>The id of the event the category belongs to.</summary>
        public int EventID { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        /// <summary>The name of the category.</summary>
        public string CategoryName { get; set; }
    }
}