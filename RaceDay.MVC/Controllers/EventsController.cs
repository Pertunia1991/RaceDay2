
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public EventsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY ALL EVENTS
        public async Task<IActionResult> Index()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var events = await _apiService.GetEventsAsync(token);
                return View(events);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve events. Please try again.";

                return View(new List<EventViewModel>());
            }
        }

        // DISPLAY EVENT DETAILS AND CATEGORIES
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var eventItem = await _apiService.GetEventAsync(id, token);

                if (eventItem == null)
                    return NotFound();

                var categories =
                    await _apiService.GetCategoriesByEventAsync(id, token);

                ViewBag.Categories = categories;
                ViewBag.Role = role;

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve event details or categories.";

                return RedirectToAction("Index");
            }
        }

        // DISPLAY CREATE EVENT FORM
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            await LoadEventTypesAsync();
            return View();
        }

        // CREATE EVENT AND UPLOAD OPTIONAL BANNER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EventViewModel eventItem,
            IFormFile? banner)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            if (!ModelState.IsValid)
            {
                await LoadEventTypesAsync();
                return View(eventItem);
            }

            if (!ValidateBanner(banner))
            {
                await LoadEventTypesAsync();
                return View(eventItem);
            }

            eventItem.OrganiserID = userID!.Value;

            try
            {
                var success = await _apiService.CreateEventAsync(
                    eventItem, token!);

                if (success)
                {
                    var events = await _apiService.GetEventsAsync(token!);

                    var createdEvent = events
                        .Where(e =>
                            e.OrganiserID == userID.Value &&
                            e.EventName == eventItem.EventName &&
                            e.EventDate == eventItem.EventDate)
                        .OrderByDescending(e => e.EventID)
                        .FirstOrDefault();

                    if (banner != null && banner.Length > 0)
                    {
                        if (createdEvent == null)
                        {
                            TempData["ErrorMessage"] =
                                "The event was created, but its banner could not be linked.";

                            return RedirectToAction("Index");
                        }

                        var imageUrl = await _apiService.UploadFileAsync(
                            $"api/Events/{createdEvent.EventID}/banner",
                            banner,
                            token!);

                        if (string.IsNullOrWhiteSpace(imageUrl))
                        {
                            TempData["ErrorMessage"] =
                                "The event was created, but the banner upload failed.";

                            return RedirectToAction("Index");
                        }
                    }

                    TempData["SuccessMessage"] =
                        "Event created successfully.";

                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(
                    string.Empty,
                    "The event could not be created. Please check your details.");

                await LoadEventTypesAsync();
                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay API.");

                await LoadEventTypesAsync();
                return View(eventItem);
            }
        }

        // DISPLAY EDIT EVENT FORM
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                var eventItem = await _apiService.GetEventAsync(id, token!);

                if (eventItem == null)
                    return NotFound();

                if (eventItem.OrganiserID != userID)
                    return Forbid();

                await LoadEventTypesAsync();

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve the event for editing.";

                return RedirectToAction("Index");
            }
        }

        // SAVE EVENT CHANGES
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EventViewModel eventItem,
            IFormFile? banner)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            if (id != eventItem.EventID)
                return BadRequest();

            try
            {
                var existingEvent =
                    await _apiService.GetEventAsync(id, token!);

                if (existingEvent == null)
                    return NotFound();

                if (existingEvent.OrganiserID != userID)
                    return Forbid();

                if (!ModelState.IsValid)
                {
                    await LoadEventTypesAsync();
                    return View(eventItem);
                }

                if (!ValidateBanner(banner))
                {
                    await LoadEventTypesAsync();
                    return View(eventItem);
                }

                // Preserve the original organiser.
                eventItem.OrganiserID = existingEvent.OrganiserID;

                // Preserve the existing banner if no new one is selected.
                eventItem.BannerImageURL = existingEvent.BannerImageURL;

                var success = await _apiService.UpdateEventAsync(
                    id, eventItem, token!);

                if (!success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The event could not be updated.");

                    await LoadEventTypesAsync();
                    return View(eventItem);
                }

                // Upload a replacement banner if selected.
                if (banner != null && banner.Length > 0)
                {
                    var imageUrl = await _apiService.UploadFileAsync(
                        $"api/Events/{id}/banner",
                        banner,
                        token!);

                    if (string.IsNullOrWhiteSpace(imageUrl))
                    {
                        TempData["ErrorMessage"] =
                            "Event details were updated, but the new banner could not be uploaded.";

                        return RedirectToAction("Index");
                    }
                }

                TempData["SuccessMessage"] =
                    "Event updated successfully.";

                return RedirectToAction("Index");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay API.");

                await LoadEventTypesAsync();
                return View(eventItem);
            }
        }

        // DISPLAY DELETE CONFIRMATION
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                var eventItem = await _apiService.GetEventAsync(id, token!);

                if (eventItem == null)
                    return NotFound();

                if (eventItem.OrganiserID != userID)
                    return Forbid();

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve the event.";

                return RedirectToAction("Index");
            }
        }

        // DELETE EVENT
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                var eventItem = await _apiService.GetEventAsync(id, token!);

                if (eventItem == null)
                    return NotFound();

                if (eventItem.OrganiserID != userID)
                    return Forbid();

                var success = await _apiService.DeleteEventAsync(id, token!);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "The event could not be deleted. It may have existing categories or enrolments.";

                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] =
                    "Event deleted successfully.";

                return RedirectToAction("Index");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index");
            }
        }

        // PROCESS PARTICIPANT ENROLMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(EnrolmentRequest request)
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null ||
                role != "Participant" ||
                string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (request.EventID <= 0 || request.CategoryID <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a valid event and category.";

                return RedirectToAction("Index");
            }

            try
            {
                var success = await _apiService.CreateEnrolmentAsync(
                    request, token);

                if (!success)
                {
                    TempData["ErrorMessage"] =
                        "You are already enrolled in this event.";

                    return RedirectToAction("Details",
                        new { id = request.EventID });
                }

                TempData["SuccessMessage"] =
                    "You have successfully enrolled in this event!";

                return RedirectToAction("Index", "Enrolments");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to complete your enrolment. Please try again.";

                return RedirectToAction("Details",
                    new { id = request.EventID });
            }
        }

        // VALIDATE OPTIONAL EVENT BANNER
        private bool ValidateBanner(IFormFile? banner)
        {
            if (banner == null || banner.Length == 0)
                return true;

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedTypes.Contains(
                banner.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    "banner",
                    "Only JPG, PNG, GIF and WEBP images are allowed.");

                return false;
            }

            if (banner.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "banner",
                    "The banner image must not exceed 5 MB.");

                return false;
            }

            return true;
        }

        // LOAD EVENT TYPES INTO DROPDOWN
        private async Task LoadEventTypesAsync()
        {
            var eventTypes = await _apiService.GetEventTypesAsync();

            ViewBag.EventTypes = new SelectList(
                eventTypes,
                "EventTypeID",
                "TypeName");
        }

        // CHECK WHETHER LOGGED-IN USER IS AN ORGANISER
        private bool IsOrganiser()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");
            var token = HttpContext.Session.GetString("JWToken");

            return userID != null
                && role == "Organiser"
                && !string.IsNullOrWhiteSpace(token);
        }
    }
}
