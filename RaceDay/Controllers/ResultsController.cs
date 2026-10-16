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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private static IQueryable<ResultDto> ToDto(IQueryable<Result> query)
        {
            return query.Select(r => new ResultDto
            {
                ResultsID = r.ResultsID,
                EnrollID = r.EnrollID,
                EventID = r.Enrollment.EventID,
                EventName = r.Enrollment.Event.EventName,
                CateID = r.CateID,
                CategoryName = r.Category.CategoryName,
                PNumber = r.Enrollment.PNumber,
                ParticipantName = r.Enrollment.Participant.Name + " " + r.Enrollment.Participant.Surname,
                ResultsStatus = r.ResultsStatus,
                FinishTime = r.FinishTime,
                Position = r.Position
            });
        }

        // Returns an error message if the status and position don't make sense together
        private string CheckResultRules(UpdateResultDto dto)
        {
            if (dto.ResultsStatus == "Finished" && dto.Position < 1)
            {
                return "A finished participant needs a position of 1 or higher.";
            }

            if (dto.ResultsStatus != "Finished" && dto.Position != 0)
            {
                return "Only finished participants can have a position.";
            }

            return null;
        }

        // Is this position already taken by someone else in the same event and category?
        private async Task<bool> PositionTaken(Enrollment enrollment, int position, int ignoreResultId)
        {
            if (position == 0)
            {
                return false;
            }

            return await _context.Results.AnyAsync(r =>
                r.ResultsID != ignoreResultId &&
                r.Enrollment.EventID == enrollment.EventID &&
                r.CateID == enrollment.CateID &&
                r.Position == position);
        }

        // POST: api/results  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<ResultDto>> CaptureResult(CreateResultDto dto)
        {
            Enrollment enrollment = await _context.Enrollments.FindAsync(dto.EnrollID);

            if (enrollment == null)
            {
                return NotFound("Enrolment not found.");
            }

            Event parentEvent = await _context.Events.FindAsync(enrollment.EventID);

            if (parentEvent.OrganiserCode != GetUserId())
            {
                return Forbid();
            }

            string ruleError = CheckResultRules(dto);

            if (ruleError != null)
            {
                return BadRequest(ruleError);
            }

            bool resultExists = await _context.Results.AnyAsync(r => r.EnrollID == dto.EnrollID);

            if (resultExists)
            {
                return Conflict("A result has already been captured for this enrolment.");
            }

            if (await PositionTaken(enrollment, dto.Position, 0))
            {
                return Conflict("That position is already taken in this category.");
            }

            Result result = new Result
            {
                EnrollID = dto.EnrollID,
                CateID = enrollment.CateID,
                ResultsStatus = dto.ResultsStatus,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            ResultDto created = await ToDto(_context.Results.Where(r => r.ResultsID == result.ResultsID)).FirstAsync();

            return CreatedAtAction(nameof(GetResult), new { id = result.ResultsID }, created);
        }

        // PUT: api/results/1  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResultDto>> UpdateResult(int id, UpdateResultDto dto)
        {
            Result result = await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            Enrollment enrollment = await _context.Enrollments.FindAsync(result.EnrollID);
            Event parentEvent = await _context.Events.FindAsync(enrollment.EventID);

            if (parentEvent.OrganiserCode != GetUserId())
            {
                return Forbid();
            }

            string ruleError = CheckResultRules(dto);

            if (ruleError != null)
            {
                return BadRequest(ruleError);
            }

            if (await PositionTaken(enrollment, dto.Position, id))
            {
                return Conflict("That position is already taken in this category.");
            }

            result.ResultsStatus = dto.ResultsStatus;
            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;

            await _context.SaveChangesAsync();

            return await ToDto(_context.Results.Where(r => r.ResultsID == id)).FirstAsync();
        }

        // GET: api/results/my  (Participant only, own results)
        [Authorize(Roles = "Participant")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetMyResults()
        {
            int pNumber = GetUserId();

            return await ToDto(_context.Results.Where(r => r.Enrollment.PNumber == pNumber)).ToListAsync();
        }

        // GET: api/results/event/1  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsForEvent(int eventId)
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

            return await ToDto(_context.Results.Where(r => r.Enrollment.EventID == eventId)).ToListAsync();
        }

        // GET: api/results/1  (the participant it belongs to, or the organiser of the event)
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto>> GetResult(int id)
        {
            Result result = await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            Enrollment enrollment = await _context.Enrollments.FindAsync(result.EnrollID);
            Event parentEvent = await _context.Events.FindAsync(enrollment.EventID);
            int userId = GetUserId();

            bool isOwnResult = User.IsInRole("Participant") && enrollment.PNumber == userId;
            bool isEventOrganiser = User.IsInRole("Organiser") && parentEvent.OrganiserCode == userId;

            if (!isOwnResult && !isEventOrganiser)
            {
                return Forbid();
            }

            return await ToDto(_context.Results.Where(r => r.ResultsID == id)).FirstAsync();
        }
    }
}