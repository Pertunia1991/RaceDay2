
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        // GET: api/Categories/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(
            Category category)
        {
            if (!await _context.Events.AnyAsync(
                e => e.EventID == category.EventID))
            {
                return BadRequest("The specified event does not exist.");
            }

            category.Event = null;
            category.Enrolments = new List<Enrolment>();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // PUT: api/Categories/1
        [HttpPut("{id:int}")]
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
