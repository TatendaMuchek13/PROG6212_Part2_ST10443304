using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IConfiguration _config;

        public AuthController(RaceDayDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // POST: api/auth/register
        [AllowAnonymous]
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

        // POST: api/auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            string role = null;
            int userId = 0;
            string hash = null;

            Organiser organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.Email == dto.Email);

            if (organiser != null)
            {
                role = "Organiser";
                userId = organiser.OrganiserCode;
                hash = organiser.Password;
            }
            else
            {
                Participant participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == dto.Email);

                if (participant != null)
                {
                    role = "Participant";
                    userId = participant.PNumber;
                    hash = participant.Password;
                }
            }

            if (role == null || !BCrypt.Net.BCrypt.Verify(dto.Password, hash))
            {
                return Unauthorized("Invalid email or password.");
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Role, role)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = jwt, role = role });
        }

        

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string role = User.FindFirstValue(ClaimTypes.Role);

            return Ok("You are user " + id + " with the role " + role + ".");
        }

        [Authorize(Roles = "Organiser")]
        [HttpGet("organiser-only")]
        public IActionResult OrganiserOnly()
        {
            return Ok("Welcome, organiser.");
        }
    }
}