
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Data;
using RaceDay2.API.Models;
using RaceDay2.API.Services;

namespace RaceDay2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

        private int? GetCurrentUserID()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int id))
                return id;

            return null;
        }

        // GET: api/Users
        // ORGANISER: VIEW ALL USERS
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            return Ok(users.Select(ToResponse));
        }

        // GET: api/Users/1
        // USERS: VIEW OWN PROFILE
        // ORGANISER: VIEW ANY PROFILE
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var currentUserID = GetCurrentUserID();

            if (currentUserID == null)
                return Unauthorized();

            if (currentUserID != id &&
                !User.IsInRole("Organiser"))
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return Ok(ToResponse(user));
        }

        // POST: api/Users
        // PUBLIC USER REGISTRATION
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> RegisterUser(
            RegisterUserRequest request)
        {
            if (await _context.Users.AnyAsync(
                u => u.Email == request.Email))
            {
                return Conflict(
                    "An account with this email already exists.");
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
        // USERS: UPDATE OWN PROFILE
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(
            int id, UpdateUserRequest request)
        {
            var currentUserID = GetCurrentUserID();

            if (currentUserID == null)
                return Unauthorized();

            if (currentUserID != id)
                return Forbid();

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            if (await _context.Users.AnyAsync(
                u => u.Email == request.Email && u.UserID != id))
            {
                return Conflict(
                    "That email address is already in use.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.ProfilePictureURL = request.ProfilePictureURL;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users/1/profile-picture
        // USERS: UPLOAD OWN PROFILE PICTURE
        [HttpPost("{id:int}/profile-picture")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadProfilePicture(
            int id,
            [FromForm] IFormFile file,
            [FromServices] BlobStorageService blobStorage)
        {
            var currentUserID = GetCurrentUserID();

            if (currentUserID == null)
                return Unauthorized();

            if (currentUserID != id)
                return Forbid();

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            if (file == null || file.Length == 0)
                return BadRequest("Please select a profile picture.");

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
                    "profile-pictures");

                user.ProfilePictureURL = url;

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

        // DELETE: api/Users/1
        // ORGANISER ONLY
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
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
