
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventTypesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventTypesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventType>>> GetEventTypes()
        {
            return await _context.EventTypes.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventType>> GetEventType(int id)
        {
            var item = await _context.EventTypes.FindAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<EventType>> CreateEventType(
            EventType item)
        {
            if (await _context.EventTypes.AnyAsync(
                t => t.TypeName == item.TypeName))
            {
                return Conflict("This event type already exists.");
            }

            item.EventTypeID = 0;
            item.Events = new List<Event>();

            _context.EventTypes.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventType),
                new { id = item.EventTypeID },
                item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEventType(
            int id, EventType item)
        {
            if (id != item.EventTypeID)
                return BadRequest("The event type ID does not match.");

            var existing = await _context.EventTypes.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (await _context.EventTypes.AnyAsync(
                t => t.TypeName == item.TypeName &&
                     t.EventTypeID != id))
            {
                return Conflict("This event type name is already in use.");
            }

            existing.TypeName = item.TypeName;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var item = await _context.EventTypes.FindAsync(id);

            if (item == null)
                return NotFound();

            if (await _context.Events.AnyAsync(
                e => e.EventTypeID == id))
            {
                return Conflict(
                    "Cannot delete an event type that is in use.");
            }

            _context.EventTypes.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
