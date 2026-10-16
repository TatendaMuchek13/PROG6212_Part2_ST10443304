using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The new name for an existing category.</summary>
    public class UpdateCategoryDto
    {
        /// <summary>The new name of the category. Maximum 100 characters.</summary>
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
    }
}