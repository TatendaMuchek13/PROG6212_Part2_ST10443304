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
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrollmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // Turns enrolment rows into safe, readable response objects
        private static IQueryable<EnrollmentDto> ToDto(IQueryable<Enrollment> query)
        {
            return query.Select(e => new EnrollmentDto
            {
                EnrollID = e.EnrollID,
                EventID = e.EventID,
                EventName = e.Event.EventName,
                CateID = e.CateID,
                CategoryName = e.Category.CategoryName,
                PNumber = e.PNumber,
                ParticipantName = e.Participant.Name + " " + e.Participant.Surname,
                DateOfEnroll = e.DateOfEnroll
            });
        }

        // POST: api/enrollments  (Participant only)
        [Authorize(Roles = "Participant")]
        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> Enroll(CreateEnrollmentDto dto)
        {
            int pNumber = GetUserId();

            Event existingEvent = await _context.Events.FindAsync(dto.EventID);

            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            Category category = await _context.Categories.FindAsync(dto.CateID);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            if (category.EventID != dto.EventID)
            {
                return BadRequest("That category does not belong to this event.");
            }

            bool alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.PNumber == pNumber && e.EventID == dto.EventID);

            if (alreadyEnrolled)
            {
                return Conflict("You are already enrolled in this event.");
            }

            Enrollment enrollment = new Enrollment
            {
                PNumber = pNumber,
                EventID = dto.EventID,
                CateID = dto.CateID,
                DateOfEnroll = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            EnrollmentDto result = await ToDto(_context.Enrollments.Where(e => e.EnrollID == enrollment.EnrollID))
                .FirstAsync();

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.EnrollID }, result);
        }

        // GET: api/enrollments/my  (Participant only, own enrolments)
        [Authorize(Roles = "Participant")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments()
        {
            int pNumber = GetUserId();

            return await ToDto(_context.Enrollments.Where(e => e.PNumber == pNumber)).ToListAsync();
        }

        // GET: api/enrollments  (Organiser only, enrolments for all their events)
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsForMyEvents()
        {
            int organiserCode = GetUserId();

            return await ToDto(_context.Enrollments.Where(e => e.Event.OrganiserCode == organiserCode)).ToListAsync();
        }

        // GET: api/enrollments/event/1  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsForEvent(int eventId)
        {
            Event existingEvent = await _context.Events.FindAsync(eventId);

            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            if (existingEvent.OrganiserCode != GetUserId())
            {
                return Forbid();
            }

            return await ToDto(_context.Enrollments.Where(e => e.EventID == eventId)).ToListAsync();
        }

        // GET: api/enrollments/1  (the participant who owns it, or the organiser of the event)
        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
        {
            Enrollment enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            Event parentEvent = await _context.Events.FindAsync(enrollment.EventID);
            int userId = GetUserId();

            bool isEnrolledParticipant = User.IsInRole("Participant") && enrollment.PNumber == userId;
            bool isEventOrganiser = User.IsInRole("Organiser") && parentEvent.OrganiserCode == userId;

            if (!isEnrolledParticipant && !isEventOrganiser)
            {
                return Forbid();
            }

            return await ToDto(_context.Enrollments.Where(e => e.EnrollID == id)).FirstAsync();
        }
    }
}