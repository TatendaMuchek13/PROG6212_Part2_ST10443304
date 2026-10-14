using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Models
{
    public class Organiser
    {
        [Key] public int OrganiserCode { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Cell { get; set; }
        public string Email { get; set; }
        public string Password{ get; set; }
    }g
}
