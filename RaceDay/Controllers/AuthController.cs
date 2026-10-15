using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.DTOs;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Role != "Organiser" && dto.Role != "Participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            bool emailTaken = await _context.Participants.AnyAsync(p => p.Email == dto.Email)
                           || await _context.Organisers.AnyAsync(o => o.Email == dto.Email);

            if (emailTaken)
            {
                return Conflict("That email is already registered.");
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            if (dto.Role == "Organiser")
            {
                Organiser organiser = new Organiser
                {
                    Name = dto.Name,
                    Surname = dto.Surname,
                    Cell = dto.Cell,
                    Email = dto.Email,
                    Password = hash
                };

                _context.Organisers.Add(organiser);
            }
            else
            {
                Participant participant = new Participant
                {
                    Name = dto.Name,
                    Surname = dto.Surname,
                    Cell = dto.Cell,
                    Email = dto.Email,
                    Password = hash
                };

                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return Ok("Registered successfully as " + dto.Role + ".");
        }
    }
}
