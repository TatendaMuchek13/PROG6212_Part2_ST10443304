using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class CreateCategoryDto
    {
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }      // e.g. Under 20, Senior, 10km
    }
}