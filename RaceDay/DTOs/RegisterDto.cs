using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Cell { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]// The length convension
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }  // "Organiser" or "Participant" to sign in
    }
}