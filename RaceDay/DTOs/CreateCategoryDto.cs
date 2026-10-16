using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The details of a new category for an event.</summary>
    public class CreateCategoryDto
    {
        /// <summary>The id of the event the category belongs to. The event must belong to the logged-in Organiser.</summary>
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        /// <summary>The name of the category, for example Under 20, Senior, 10km or 21km. Maximum 100 characters.</summary>
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
    }
}