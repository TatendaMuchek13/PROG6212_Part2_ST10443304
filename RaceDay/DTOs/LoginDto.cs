using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    /// <summary>The credentials used to log in.</summary>
    public class LoginDto
    {
        /// <summary>The email address the account was registered with.</summary>
        [Required]
        public string Email { get; set; }

        /// <summary>The account password.</summary>
        [Required]
        public string Password { get; set; }
    }
}