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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Gets all categories across all events.
        /// </summary>
        /// <remarks>Available to any logged-in user. Expects no request body.</remarks>
        /// <response code="200">Returns the list of categories.</response>
        /// <response code="401">The user is not logged in.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        /// <summary>
        /// Gets a single category by its id.
        /// </summary>
        /// <remarks>Available to any logged-in user. The id is taken from the address.</remarks>
        /// <param name="id">The id of the category.</param>
        /// <response code="200">Returns the category.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="404">No category has that id.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            Category category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        /// <summary>
        /// Gets all categories that belong to one event.
        /// </summary>
        /// <remarks>
        /// Available to any logged-in user, so participants can see which categories they can
        /// enter. The event id is taken from the address.
        /// </remarks>
        /// <param name="eventId">The id of the event.</param>
        /// <response code="200">Returns the categories for that event. The list is empty if there are none.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="404">No event has that id.</response>
        [HttpGet("event/{eventId}")]
        [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesForEvent(int eventId)
        {
            Event existingEvent = await _context.Events.FindAsync(eventId);

            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            return await _context.Categories.Where(c => c.EventID == eventId).ToListAsync();
        }

        /// <summary>
        /// Creates a new category for an event that the logged-in Organiser owns.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for their own events. Expects a JSON body with eventID and
        /// categoryName (for example Under 20, Senior, 10km or 21km). An event cannot have two
        /// categories with the same name.
        /// </remarks>
        /// <param name="dto">The event id and the name of the new category.</param>
        /// <response code="201">The category was created. Returns the new category.</response>
        /// <response code="400">A field is missing or invalid.</response>
        /// <response code="401">The user is not logged in.</response>
        /// <response code="403">The user is not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">No event has that id.</response>
        /// <response code="409">The event already has a category with that name.</response>
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Category>> CreateCategory(CreateCategoryDto dto)
        {
            Event existingEvent = await _context.Events.FindAsync(dto.EventID);

            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            if (existingEvent.OrganiserCode != GetUserId())
            {
                return Forbid();
            }

            bool nameTaken = await _context.Categories
                .AnyAsync(c => c.EventID == dto.EventID && c.CategoryName == dto.CategoryName);

            if (nameTaken)
            {
                return Conflict("This event already has a category with that name.");
            }

            Category category = new Category
            {
                EventID = dto.EventID,
                CategoryName = dto.CategoryName
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.CateID }, category);
        }
    } }

        /// <summary>
        /// Renames a category of an event that the logged-in Organiser owns.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for their own events. Expects a JSON body with the new
        /// categoryName. A category cannot be