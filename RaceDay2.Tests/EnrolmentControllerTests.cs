
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Controllers;
using RaceDay2.API.Data;
using RaceDay2.API.Models;
using Xunit;

namespace RaceDay2.Tests
{
    public class EnrolmentsControllerTests
    {
        private readonly RaceDayDbContext _context;
        private readonly EnrolmentsController _controller;

        public EnrolmentsControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);
            _controller = new EnrolmentsController(_context);
        }

        private async Task SeedDataAsync()
        {
            _context.Users.Add(new User
            {
                UserID = 1,
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                PasswordHash = "test-hash",
                Role = "Participant",
                CreatedDate = DateTime.UtcNow
            });

            _context.EventTypes.Add(new EventType
            {
                EventTypeID = 1,
                TypeName = "Running"
            });

            await _context.SaveChangesAsync();

            _context.Events.Add(new Event
            {
                EventID = 1,
                OrganiserID = 1,
                EventTypeID = 1,
                EventName = "Test Run",
                Description = "Test event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Johannesburg",
                Distance = 10
            });

            await _context.SaveChangesAsync();

            _context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10km",
                Distance = 10
            });

            await _context.SaveChangesAsync();
        }

        private static Enrolment MakeEnrolment()
        {
            return new Enrolment
            {
                ParticipantID = 1,
                EventID = 1,
                CategoryID = 1,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Registered"
            };
        }
        [Fact]
        public async Task GetEnrolments_ReturnsEmptyList_WhenNoneExist()
        {
            var result = await _controller.GetEnrolments();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var enrolments = Assert.IsAssignableFrom<IEnumerable<Enrolment>>(
                okResult.Value);

            Assert.Empty(enrolments);
        }
        [Fact]
        public async Task GetEnrolment_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.GetEnrolment(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateEnrolment_ReturnsBadRequest_WhenParticipantMissing()
        {
            await SeedDataAsync();

            var item = MakeEnrolment();
            item.ParticipantID = 999;

            var result = await _controller.CreateEnrolment(item);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateEnrolment_ReturnsBadRequest_WhenEventMissing()
        {
            await SeedDataAsync();

            var item = MakeEnrolment();
            item.EventID = 999;

            var result = await _controller.CreateEnrolment(item);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateEnrolment_ReturnsCreated_WhenDataIsValid()
        {
            await SeedDataAsync();

            var result = await _controller.CreateEnrolment(
                MakeEnrolment());

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Single(await _context.Enrolments.ToListAsync());
        }

        [Fact]
        public async Task CreateEnrolment_ReturnsBadRequest_WhenCategoryBelongsToAnotherEvent()
        {
            await SeedDataAsync();

            _context.Events.Add(new Event
            {
                EventID = 2,
                OrganiserID = 1,
                EventTypeID = 1,
                EventName = "Second Run",
                Description = "Another test event",
                EventDate = DateTime.UtcNow.AddDays(60),
                Location = "Pretoria",
                Distance = 5
            });

            _context.Categories.Add(new Category
            {
                CategoryID = 2,
                EventID = 2,
                CategoryName = "5km",
                Distance = 5
            });

            await _context.SaveChangesAsync();

            var item = MakeEnrolment();
            item.CategoryID = 2;

            var result = await _controller.CreateEnrolment(item);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateEnrolment_ReturnsNotFound_WhenMissing()
        {
            var item = MakeEnrolment();
            item.EnrolmentID = 999;

            var result = await _controller.UpdateEnrolment(999, item);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateEnrolment_ReturnsNoContent_WhenValid()
        {
            await SeedDataAsync();

            var item = MakeEnrolment();
            _context.Enrolments.Add(item);
            await _context.SaveChangesAsync();

            item.Status = "Confirmed";

            var result = await _controller.UpdateEnrolment(
                item.EnrolmentID, item);

            Assert.IsType<NoContentResult>(result);

            var updated = await _context.Enrolments.FindAsync(
                item.EnrolmentID);

            Assert.Equal("Confirmed", updated!.Status);
        }

        [Fact]
        public async Task DeleteEnrolment_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.DeleteEnrolment(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteEnrolment_ReturnsNoContent_WhenNoResultExists()
        {
            await SeedDataAsync();

            var item = MakeEnrolment();
            _context.Enrolments.Add(item);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteEnrolment(
                item.EnrolmentID);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(await _context.Enrolments.ToListAsync());
        }
    }
}
