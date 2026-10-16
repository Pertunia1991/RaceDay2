
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            return await _context.Enrolments.ToListAsync();
        }

        // GET: api/Enrolments/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound();

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        [HttpPost]
        public async Task<ActionResult<Enrolment>> CreateEnrolment(
            Enrolment enrolment)
        {
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

            var category = await _context.Categories.FindAsync(
                enrolment.CategoryID);

            if (category == null ||
                category.EventID != enrolment.EventID)
            {
                return BadRequest(
                    "The category does not belong to this event.");
            }

            enrolment.EnrolmentID = 0;
            enrolment.Participant = null;
            enrolment.Event = null;
            enrolment.Category = null;
            enrolment.Result = null;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment);
        }

        // PUT: api/Enrolments/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEnrolment(
            int id, Enrolment enrolment)
        {
            if (id != enrolment.EnrolmentID)
                return BadRequest("The enrolment ID does not match.");

            var existing = await _context.Enrolments.FindAsync(id);

            if (existing == null)
                return NotFound();

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

            var category = await _context.Categories.FindAsync(
                enrolment.CategoryID);

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

        // DELETE: api/Enrolments/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound();

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
