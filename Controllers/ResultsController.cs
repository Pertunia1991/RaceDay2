
using System.Security.Claims;
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ORGANISER: VIEW ALL RESULTS
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    ParticipantID = r.Enrolment!.ParticipantID,
                    EventID = r.Enrolment.EventID,
                    EventName = r.Enrolment.Event!.EventName,
                    CategoryName = r.Enrolment.Category!.CategoryName,
                    r.FinishTime,
                    r.FinishPosition,
                    r.ResultDate,
                    r.IsPublished
                })
                .ToListAsync();

            return Ok(results);
        }

        // ORGANISER: VIEW RESULTS FOR ONE EVENT
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _context.Results
                .Where(r => r.Enrolment!.EventID == eventId)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    ParticipantID = r.Enrolment!.ParticipantID,
                    EventID = r.Enrolment.EventID,
                    EventName = r.Enrolment.Event!.EventName,
                    CategoryName = r.Enrolment.Category!.CategoryName,
                    r.FinishTime,
                    r.FinishPosition,
                    r.ResultDate,
                    r.IsPublished
                })
                .ToListAsync();

            return Ok(results);
        }

        // ORGANISER: CAPTURE A RESULT
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(Result result)
        {
            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentID == result.EnrolmentID);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            var existingResult = await _context.Results
                .AnyAsync(r => r.EnrolmentID == result.EnrolmentID);

            if (existingResult)
            {
                return Conflict("A result already exists for this enrolment.");
            }

            if (result.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest("Finish time must be greater than zero.");
            }

            if (result.FinishPosition <= 0)
            {
                return BadRequest("Finish position must be greater than zero.");
            }

            result.ResultDate = DateTime.UtcNow;
            result.IsPublished = false;
            result.Enrolment = null;

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResults),
                new { id = result.ResultID },
                result);
        }

        // ORGANISER: UPDATE A RESULT
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id, Result updatedResult)
        {
            if (id != updatedResult.ResultID)
            {
                return BadRequest("Result ID mismatch.");
            }

            var existingResult = await _context.Results
                .FindAsync(id);

            if (existingResult == null)
            {
                return NotFound("Result not found.");
            }

            if (updatedResult.FinishTime <= TimeSpan.Zero ||
                updatedResult.FinishPosition <= 0)
            {
                return BadRequest("Enter a valid finish time and position.");
            }

            existingResult.FinishTime = updatedResult.FinishTime;
            existingResult.FinishPosition = updatedResult.FinishPosition;

            await _context.SaveChangesAsync();

            return Ok(existingResult);
        }

        // ORGANISER: PUBLISH A RESULT
        [HttpPut("{id}/publish")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PublishResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound("Result not found.");
            }

            result.IsPublished = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Result published successfully.",
                result.ResultID,
                result.IsPublished
            });
        }

        // PARTICIPANT: VIEW OWN PUBLISHED RESULTS
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
            {
                return Unauthorized();
            }

            var results = await _context.Results
                .Where(r =>
                    r.Enrolment!.ParticipantID == participantId &&
                    r.IsPublished)
                .Select(r => new
                {
                    r.ResultID,
                    EventName = r.Enrolment!.Event!.EventName,
                    EventDate = r.Enrolment.Event.EventDate,
                    CategoryName = r.Enrolment.Category!.CategoryName,
                    r.FinishTime,
                    r.FinishPosition,
                    r.ResultDate
                })
                .ToListAsync();

            return Ok(results);
        }

        // ORGANISER: DELETE A RESULT
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound("Result not found.");
            }

            _context.Results.Remove(result);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
