
using System.Security.Claims;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;
using RaceDay2.API.Services;

namespace RaceDay2.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/Events/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var item = await _context.Events.FindAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST: api/Events
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event item)
        {
            if (!await _context.Users.AnyAsync(
                u => u.UserID == item.OrganiserID))
            {
                return BadRequest("The organiser does not exist.");
            }

            if (!await _context.EventTypes.AnyAsync(
                t => t.EventTypeID == item.EventTypeID))
            {
                return BadRequest("The event type does not exist.");
            }

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

        // PUT: api/Events/1
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(
            int id, Event item)
        {
            if (id != item.EventID)
                return BadRequest("The event ID does not match.");

            var existing = await _context.Events.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (!await _context.Users.AnyAsync(
                u => u.UserID == item.OrganiserID))
            {
                return BadRequest("The organiser does not exist.");
            }

            if (!await _context.EventTypes.AnyAsync(
                t => t.EventTypeID == item.EventTypeID))
            {
                return BadRequest("The event type does not exist.");
            }

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

        // DELETE: api/Events/1
        [Authorize(Roles = "Organiser")]
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

        // POST: api/Events/1/banner
        // Only the Organiser who owns the event can upload its banner.
        [Authorize(Roles = "Organiser")]
        [HttpPost("{id:int}/banner")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadBanner(
            int id,
            IFormFile file,
            [FromServices] BlobStorageService blobStorage)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized();

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserID != currentUserId)
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("Please select an image.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image must not exceed 5 MB.");

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedTypes.Contains(
                file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(
                    "Only JPG, PNG, GIF and WEBP images are allowed.");
            }

            try
            {
                string url = await blobStorage.UploadAsync(
                    file,
                    "event-banners");

                eventItem.BannerImageURL = url;

                await _context.SaveChangesAsync();

                return Ok(new { imageUrl = url });
            }
            catch (RequestFailedException)
            {
                return StatusCode(
                    503,
                    "Azure Blob Storage is currently unavailable.");
            }
        }
    }
}
