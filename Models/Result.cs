using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        public int ResultsID { get; set; }

        public int EnrollID { get; set; }
        [ForeignKey("EnrollID")]
        public Enrollment? Enrollment { get; set; }

        public int CateID { get; set; }
        [ForeignKey("CateID")]
        public Category? Category { get; set; }

        public string ResultsStatus { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }
}