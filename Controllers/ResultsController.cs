
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            return await _context.Results.ToListAsync();
        }

        // GET: api/Results/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/Results
        [HttpPost]
        public async Task<ActionResult<Result>> CreateResult(
            Result result)
        {
            if (!await _context.Enrolments.AnyAsync(
                e => e.EnrolmentID == result.EnrolmentID))
            {
                return BadRequest("The enrolment does not exist.");
            }

            if (await _context.Results.AnyAsync(
                r => r.EnrolmentID == result.EnrolmentID))
            {
                return Conflict(
                    "A result already exists for this enrolment.");
            }

            result.ResultID = 0;
            result.Enrolment = null;

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // PUT: api/Results/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateResult(
            int id, Result result)
        {
            if (id != result.ResultID)
                return BadRequest("The result ID does not match.");

            var existing = await _context.Results.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (!await _context.Enrolments.AnyAsync(
                e => e.EnrolmentID == result.EnrolmentID))
            {
                return BadRequest("The enrolment does not exist.");
            }

            if (await _context.Results.AnyAsync(
                r => r.EnrolmentID == result.EnrolmentID &&
                     r.ResultID != id))
            {
                return Conflict(
                    "Another result already exists for this enrolment.");
            }

            existing.EnrolmentID = result.EnrolmentID;
            existing.FinishTime = result.FinishTime;
            existing.FinishPosition = result.FinishPosition;
            existing.ResultDate = result.ResultDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Results/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
                return NotFound();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
