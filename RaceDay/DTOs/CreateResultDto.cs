using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTOs
{
    public class CreateResultDto : UpdateResultDto
    {
        [Range(1, int.MaxValue)]
        public int EnrollID { get; set; }
    }
}