
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;

namespace RaceDay2.API.Controllers
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

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.CategoryID,
                    c.EventID,
                    c.CategoryName,
                    c.MinimumAge,
                    c.MaximumAge,
                    c.Distance
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.CategoryID == id)
                .Select(c => new
                {
                    c.CategoryID,
                    c.EventID,
                    c.CategoryName,
                    c.MinimumAge,
                    c.MaximumAge,
                    c.Distance
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound("Category not found.");

            return Ok(category);
        }

        // GET: api/Categories/event/1
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetCategoriesByEvent(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new
                {
                    c.CategoryID,
                    c.EventID,
                    c.CategoryName,
                    c.MinimumAge,
                    c.MaximumAge,
                    c.Distance
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Categories
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!await _context.Events.AnyAsync(
                e => e.EventID == category.EventID))
            {
                return BadRequest("The specified event does not exist.");
            }

            category.CategoryID = 0;
            category.Event = null;
            category.Enrolments = new List<Enrolment>();

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                new
                {
                    category.CategoryID,
                    category.EventID,
                    category.CategoryName,
                    category.MinimumAge,
                    category.MaximumAge,
                    category.Distance
                });
        }

        // PUT: api/Categories/1
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, Category category)
        {
            if (id != category.CategoryID)
                return BadRequest("The category ID does not match.");

            var existing = await _context.Categories.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (!await _context.Events.AnyAsync(
                e => e.EventID == category.EventID))
            {
                return BadRequest("The specified event does not exist.");
            }

            existing.EventID = category.EventID;
            existing.CategoryName = category.CategoryName;
            existing.MinimumAge = category.MinimumAge;
            existing.MaximumAge = category.MaximumAge;
            existing.Distance = category.Distance;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Categories/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            if (await _context.Enrolments.AnyAsync(
                e => e.CategoryID == id))
            {
                return Conflict(
                    "Cannot delete a category that has enrolments.");
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
