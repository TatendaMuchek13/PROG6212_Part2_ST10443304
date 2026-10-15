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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events  (all logged-in user)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/events/1  (all logged-in user)
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            Event existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            return existingEvent;
        }

        // POST: api/events  (Organiser only)
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(EventDto dto)
        {
            int organiserCode = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Event newEvent = new Event
            {
                OrganiserCode = organiserCode,
                EventName = dto.EventName,
                Description = dto.Description,
                DateOfEvent = dto.DateOfEvent,
                RouteInfo = dto.RouteInfo,
                Distance = dto.Distance,
                EventType = dto.EventType
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, newEvent);
        }

        // PUT: api/events/1  (Organiser only)
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EventDto dto)
        {
            Event existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            int organiserCode = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (existingEvent.OrganiserCode != organiserCode)
            {
                return Forbid();
            }

            existingEvent.EventName = dto.EventName;
            existingEvent.Description = dto.Description;
            existingEvent.DateOfEvent = dto.DateOfEvent;
            existingEvent.RouteInfo = dto.RouteInfo;
            existingEvent.Distance = dto.Distance;
            existingEvent.EventType = dto.EventType;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/events/1  (Organiser only)
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            Event existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            int organiserCode = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (existingEvent.OrganiserCode != organiserCode)
            {
                return Forbid();
            }

            bool hasChildren = await _context.Categories.AnyAsync(c => c.EventID == id)
                            || await _context.Enrollments.AnyAsync(e => e.EventID == id);

            if (hasChildren)
            {
                return Conflict("This event has categories or enrolments and cannot be deleted.");
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return Ok("Event " + existingEvent.EventName + " has been deleted.");
        }
    }
}