using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The details needed to register a new account.</summary>
    public class RegisterDto
    {
        /// <summary>First name, for example Tatenda.</summary>
        [Required]
        public string Name { get; set; }

        /// <summary>Surname, for example Mucheki.</summary>
        [Required]
        public string Surname { get; set; }

        /// <summary>Cell number, exactly 10 digits, for example 0763849923.</summary>
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Cell must be exactly 10 digits.")]
        public string Cell { get; set; }

        /// <summary>Email address used to log in. Must not already be registered.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>Password of at least 8 characters, containing a number and a symbol. Only a hash is stored.</summary>
        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*\d)(?=.*[^a-zA-Z0-9]).*$",
            ErrorMessage = "Password must contain at least one number and one symbol.")]
        public string Password { get; set; }

        /// <summary>The account type. Must be exactly Organiser or Participant.</summary>
        [Required]
        public string Role { get; set; }
    }
}