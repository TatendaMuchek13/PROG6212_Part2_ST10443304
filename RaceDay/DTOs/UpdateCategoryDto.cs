using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
    }
}