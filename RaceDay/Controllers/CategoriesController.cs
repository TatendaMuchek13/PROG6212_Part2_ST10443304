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

        // GET: api/categories  (any logged-in user)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        // GET: api/categories/1  (any logged-in user)
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            Category category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // GET: api/categories/event/1  (any logged-in user)
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesForEvent(int eventId)
        {
            Event existingEvent = await _context.Events.FindAsync(eventId);

            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            return await _context.Categories.Where(c => c.EventID == eventId).ToListAsync();
        }

        // POST: api/categories  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpPost]
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

        // PUT: api/categories/1  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            Category category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            Event parentEvent = await _context.Events.FindAsync(category.EventID);

            if (parentEvent.OrganiserCode != GetUserId())
            {
                return Forbid();
            }

            category.CategoryName = dto.CategoryName;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // DELETE: api/categories/1  (Organiser only, own events only)
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            Event parentEvent = await _context.Events.FindAsync(category.EventID);

            if (parentEvent.OrganiserCode != GetUserId())
            {
                return Forbid();
            }

            bool inUse = await _context.Enrollments.AnyAsync(e => e.CateID == id)
                      || await _context.Results.AnyAsync(r => r.CateID == id);

            if (inUse)
            {
                return Conflict("This category has enrolments or results and cannot be deleted.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok("Category " + category.CategoryName + " has been deleted.");
        }
    }
}