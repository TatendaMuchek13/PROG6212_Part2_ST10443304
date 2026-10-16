using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The profile details a user can change.</summary>
    public class UpdateProfileDto
    {
        /// <summary>First name. Maximum 50 characters.</summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>Surname. Maximum 70 characters.</summary>
        [Required]
        [StringLength(70)]
        public string Surname { get; set; }

        /// <summary>Cell number, exactly 10 digits.</summary>
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Cell must be exactly 10 digits.")]
        public string Cell { get; set; }

        /// <summary>Email address. Must not belong to another account.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}