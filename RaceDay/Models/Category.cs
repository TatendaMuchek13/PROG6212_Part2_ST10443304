using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    public class Category
    {
       
        [Key] public int CateID { get; set; }
       [ForeignKey("EventID")] public int EventID { get; set; }
       
        public Event? Event { get; set; } //Refenences the Event Table

        public string CategoryName { get; set; }
    }
}
