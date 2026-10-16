
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        public class RegisterUserRequest
        {
            [Required, MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required, MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress, MaxLength(100)]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(8)]
            public string Password { get; set; } = string.Empty;

            [MaxLength(20)]
            public string? PhoneNumber { get; set; }
        }

        public class UpdateUserRequest
        {
            [Required, MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required, MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress, MaxLength(100)]
            public string Email { get; set; } = string.Empty;

            [MaxLength(20)]
            public string? PhoneNumber { get; set; }

            [MaxLength(500)]
            public string? ProfilePictureURL { get; set; }
        }

        public class UserResponse
        {
            public int UserID { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string Role { get; set; } = string.Empty;
            public string? ProfilePictureURL { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        private static UserResponse ToResponse(User user)
        {
            return new UserResponse
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                ProfilePictureURL = user.ProfilePictureURL,
                CreatedDate = user.CreatedDate
            };
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            return Ok(users.Select(ToResponse));
        }

        // GET: api/Users/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return Ok(ToResponse(user));
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserResponse>> RegisterUser(
            RegisterUserRequest request)
        {
            if (await _context.Users.AnyAsync(
                u => u.Email == request.Email))
            {
                return Conflict("An account with this email already exists.");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = "Participant",
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = ToResponse(user);

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserID },
                response);
        }

        // PUT: api/Users/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(
            int id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            if (await _context.Users.AnyAsync(
                u => u.Email == request.Email && u.UserID != id))
            {
                return Conflict("That email address is already in use.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.ProfilePictureURL = request.ProfilePictureURL;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            if (await _context.Events.AnyAsync(
                e => e.OrganiserID == id) ||
                await _context.Enrolments.AnyAsync(
                e => e.ParticipantID == id))
            {
                return Conflict(
                    "Cannot delete a user who has organised events or enrolments.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

