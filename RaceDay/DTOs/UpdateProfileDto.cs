using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(70)]
        public string Surname { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Cell must be exactly 10 digits.")]
        public string Cell { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}