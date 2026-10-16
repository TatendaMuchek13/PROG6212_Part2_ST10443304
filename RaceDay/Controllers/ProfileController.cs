using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.DTOs;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: api/profile  (any logged-in user, own profile only)
        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetMyProfile()
        {
            int userId = GetUserId();

            if (User.IsInRole("Organiser"))
            {
                Organiser organiser = await _context.Organisers.FindAsync(userId);

                if (organiser == null)
                {
                    return NotFound();
                }

                return new ProfileDto
                {
                    Id = organiser.OrganiserCode,
                    Role = "Organiser",
                    Name = organiser.Name,
                    Surname = organiser.Surname,
                    Cell = organiser.Cell,
                    Email = organiser.Email
                };
            }

            Participant participant = await _context.Participants.FindAsync(userId);

            if (participant == null)
            {
                return NotFound();
            }

            return new ProfileDto
            {
                Id = participant.PNumber,
                Role = "Participant",
                Name = participant.Name,
                Surname = participant.Surname,
                Cell = participant.Cell,
                Email = participant.Email
            };
        }

        // PUT: api/profile  (any logged-in user, own profile only)
        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpdateMyProfile(UpdateProfileDto dto)
        {
            int userId = GetUserId();

            if (User.IsInRole("Organiser"))
            {
                Organiser organiser = await _context.Organisers.FindAsync(userId);

                if (organiser == null)
                {
                    return NotFound();
                }

                bool emailTaken = await _context.Organisers.AnyAsync(o => o.Email == dto.Email && o.OrganiserCode != userId)
                               || await _context.Participants.AnyAsync(p => p.Email == dto.Email);

                if (emailTaken)
                {
                    return Conflict("That email is already registered.");
                }

                organiser.Name = dto.Name;
                organiser.Surname = dto.Surname;
                organiser.Cell = dto.Cell;
                organiser.Email = dto.Email;

                await _context.SaveChangesAsync();

                return new ProfileDto
                {
                    Id = organiser.OrganiserCode,
                    Role = "Organiser",
                    Name = organiser.Name,
                    Surname = organiser.Surname,
                    Cell = organiser.Cell,
                    Email = organiser.Email
                };
            }

            Participant participant = await _context.Participants.FindAsync(userId);

            if (participant == null)
            {
                return NotFound();
            }

            bool participantEmailTaken = await _context.Participants.AnyAsync(p => p.Email == dto.Email && p.PNumber != userId)
                                      || await _context.Organisers.AnyAsync(o => o.Email == dto.Email);

            if (participantEmailTaken)
            {
                return Conflict("That email is already registered.");
            }

            participant.Name = dto.Name;
            participant.Surname = dto.Surname;
            participant.Cell = dto.Cell;
            participant.Email = dto.Email;

            await _context.SaveChangesAsync();

            return new ProfileDto
            {
                Id = participant.PNumber,
                Role = "Participant",
                Name = participant.Name,
                Surname = participant.Surname,
                Cell = participant.Cell,
                Email = participant.Email
            };
        }
    }
}