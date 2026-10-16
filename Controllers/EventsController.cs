
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var item = await _context.Events.FindAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event item)
        {
            if (!await _context.Users.AnyAsync(
                u => u.UserID == item.OrganiserID))
                return BadRequest("The organiser does not exist.");

            if (!await _context.EventTypes.AnyAsync(
                t => t.EventTypeID == item.EventTypeID))
                return BadRequest("The event type does not exist.");

            item.EventID = 0;
            item.CreatedDate = DateTime.UtcNow;
            item.Organiser = null;
            item.EventType = null;
            item.Categories = new List<Category>();
            item.Enrolments = new List<Enrolment>();

            _context.Events.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = item.EventID },
                item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, Event item)
        {
            if (id != item.EventID)
                return BadRequest("The event ID does not match.");

            var existing = await _context.Events.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (!await _context.Users.AnyAsync(
                u => u.UserID == item.OrganiserID))
                return BadRequest("The organiser does not exist.");

            if (!await _context.EventTypes.AnyAsync(
                t => t.EventTypeID == item.EventTypeID))
                return BadRequest("The event type does not exist.");

            existing.OrganiserID = item.OrganiserID;
            existing.EventTypeID = item.EventTypeID;
            existing.EventName = item.EventName;
            existing.Description = item.Description;
            existing.EventDate = item.EventDate;
            existing.Location = item.Location;
            existing.Distance = item.Distance;
            existing.BannerImageURL = item.BannerImageURL;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var item = await _context.Events.FindAsync(id);

            if (item == null)
                return NotFound();

            if (await _context.Categories.AnyAsync(
                c => c.EventID == id) ||
                await _context.Enrolments.AnyAsync(
                e => e.EventID == id))
            {
                return Conflict(
                    "Cannot delete an event that has categories or enrolments.");
            }

            _context.Events.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
