
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;
using System.Security.Claims;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Organisers can view all enrolments.
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolments()
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.ParticipantID,
                    e.EventID,
                    EventName = e.Event!.EventName,
                    e.CategoryID,
                    CategoryName = e.Category!.CategoryName,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/5
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.EnrolmentID == id)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.ParticipantID,
                    e.EventID,
                    EventName = e.Event!.EventName,
                    EventDate = e.Event.EventDate,
                    Location = e.Event.Location,
                    e.CategoryID,
                    CategoryName = e.Category!.CategoryName,
                    Distance = e.Category.Distance,
                    e.EnrolmentDate,
                    e.Status
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            return Ok(enrolment);
        }

        // GET: api/Enrolments/my
        // A participant can only view their own enrolments.
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int participantId))
                return Unauthorized();

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == participantId)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.EnrolmentDate,
                    e.Status,

                    EventID = e.EventID,
                    EventName = e.Event!.EventName,
                    EventDate = e.Event.EventDate,
                    Location = e.Event.Location,

                    CategoryID = e.CategoryID,
                    CategoryName = e.Category!.CategoryName,
                    Distance = e.Category.Distance
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/Enrolments
        // Participants can enrol themselves.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(Enrolment enrolment)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int participantId))
                return Unauthorized();

            if (!await _context.Events.AnyAsync(
                e => e.EventID == enrolment.EventID))
            {
                return BadRequest("The event does not exist.");
            }

            var category = await _context.Categories
                .FindAsync(enrolment.CategoryID);

            if (category == null ||
                category.EventID != enrolment.EventID)
            {
                return BadRequest(
                    "The selected category does not belong to this event.");
            }

            // Prevent duplicate enrolments for the same event.
            var alreadyEnrolled = await _context.Enrolments.AnyAsync(
                e => e.ParticipantID == participantId &&
                     e.EventID == enrolment.EventID);

            if (alreadyEnrolled)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            enrolment.EnrolmentID = 0;
            enrolment.ParticipantID = participantId;
            enrolment.EnrolmentDate = DateTime.UtcNow;
            enrolment.Status = "Confirmed";

            enrolment.Participant = null;
            enrolment.Event = null;
            enrolment.Category = null;
            enrolment.Result = null;

            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMyEnrolments),
                new { },
                new
                {
                    enrolment.EnrolmentID,
                    enrolment.EventID,
                    enrolment.CategoryID,
                    enrolment.EnrolmentDate,
                    enrolment.Status
                });
        }

        // PUT: api/Enrolments/5
        // Organisers can update enrolments.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEnrolment(
            int id, Enrolment enrolment)
        {
            if (id != enrolment.EnrolmentID)
                return BadRequest(
                    "The enrolment ID does not match.");

            var existing = await _context.Enrolments.FindAsync(id);

            if (existing == null)
                return NotFound("Enrolment not found.");

            if (!await _context.Users.AnyAsync(
                u => u.UserID == enrolment.ParticipantID))
            {
                return BadRequest("The participant does not exist.");
            }

            if (!await _context.Events.AnyAsync(
                e => e.EventID == enrolment.EventID))
            {
                return BadRequest("The event does not exist.");
            }

            var category = await _context.Categories
                .FindAsync(enrolment.CategoryID);

            if (category == null ||
                category.EventID != enrolment.EventID)
            {
                return BadRequest(
                    "The category does not belong to this event.");
            }

            existing.ParticipantID = enrolment.ParticipantID;
            existing.EventID = enrolment.EventID;
            existing.CategoryID = enrolment.CategoryID;
            existing.EnrolmentDate = enrolment.EnrolmentDate;
            existing.Status = enrolment.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Enrolments/5
        // Organisers can delete enrolments.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (await _context.Results.AnyAsync(
                r => r.EnrolmentID == id))
            {
                return Conflict(
                    "Delete the associated result before deleting this enrolment.");
            }

            _context.Enrolments.Remove(enrolment);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
