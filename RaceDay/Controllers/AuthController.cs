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

        /// <summary>
        /// Registers a new account as an Organiser or a Participant.
        /// </summary>
        /// <remarks>
        /// Expects a JSON body with name, surname, cell, email, password and role.
        /// The role must be "Organiser" or "Participant". The password must be at least
        /// 8 characters and contain a number and a symbol. Only a hash of the password is stored.
        /// </remarks>
        /// <param name="dto">The registration details.</param>
        /// <response code="200">The account was created.</response>
        /// <response code="400">A field is missing or invalid, or the role is not valid.</response>
        /// <response code="409">The email address is already registered.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
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

        /// <summary>
        /// Logs a user in and returns a token for use on all other endpoints.
        /// </summary>
        /// <remarks>
        /// Expects a JSON body with email and password. The response contains a token and the
        /// user's role. Send the token in the Authorization header as "Bearer" followed by the token.
        /// Tokens expire after 2 hours.
        /// </remarks>
        /// <param name="dto">The email and password.</param>
        /// <response code="200">Login succeeded. Returns the token and role.</response>
        /// <response code="400">The email or password is missing.</response>
        /// <response code="401">The email or password is incorrect.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
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
    }
}