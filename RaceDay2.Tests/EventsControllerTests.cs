

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay2.API.Controllers;
using RaceDay2.API.Data;
using RaceDay2.API.Models;
using Xunit;

namespace RaceDay2.Tests
{
    public class EventsControllerTests
    {
        private readonly RaceDayDbContext _context;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);
            _controller = new EventsController(_context);
        }

        private async Task SeedReferencesAsync()
        {
            _context.Users.Add(new User
            {
                UserID = 1,
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@example.com",
                PasswordHash = "test-hash",
                Role = "Organiser",
                CreatedDate = DateTime.UtcNow
            });

            _context.EventTypes.Add(new EventType
            {
                EventTypeID = 1
            });

            await _context.SaveChangesAsync();
        }

        private static Event MakeEvent(int id = 0)
        {
            return new Event
            {
                EventID = id,
                OrganiserID = 1,
                EventTypeID = 1,
                EventName = "Test Run",
                Description = "Unit test event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Johannesburg",
                Distance = 10
            };
        }

        [Fact]
        public async Task GetEvents_ReturnsEmptyList_WhenNoEventsExist()
        {
            var result = await _controller.GetEvents();

            Assert.NotNull(result.Value);
            Assert.Empty(result.Value!);
        }

        [Fact]
        public async Task GetEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var result = await _controller.GetEvent(99999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateEvent_ReturnsBadRequest_WhenOrganiserDoesNotExist()
        {
            var result = await _controller.CreateEvent(MakeEvent());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateEvent_ReturnsCreated_WhenReferencesExist()
        {
            await SeedReferencesAsync();

            var result = await _controller.CreateEvent(MakeEvent());

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(1, await _context.Events.CountAsync());
        }

        [Fact]
        public async Task UpdateEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var item = MakeEvent(999);

            var result = await _controller.UpdateEvent(999, item);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsNoContent_WhenEventExists()
        {
            await SeedReferencesAsync();

            var item = MakeEvent();
            _context.Events.Add(item);
            await _context.SaveChangesAsync();

            item.EventName = "Updated Test Run";

            var result = await _controller.UpdateEvent(
                item.EventID, item);

            Assert.IsType<NoContentResult>(result);

            var updated = await _context.Events.FindAsync(item.EventID);
            Assert.Equal("Updated Test Run", updated!.EventName);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var result = await _controller.DeleteEvent(99999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsNoContent_WhenEventHasNoDependencies()
        {
            await SeedReferencesAsync();

            var item = MakeEvent();
            _context.Events.Add(item);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteEvent(item.EventID);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(await _context.Events.ToListAsync());
        }
    }
}
