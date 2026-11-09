
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ResultsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // ORGANISER: VIEW RESULTS
        public async Task<IActionResult> Index(int? eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var events = await _apiService.GetEventsAsync(token);
                var enrolments = await _apiService.GetAllEnrolmentsAsync(token);
                var results = await _apiService.GetAllResultsAsync(token);

                ViewBag.Events = events;
                ViewBag.SelectedEventID = eventId;

                var selectedEnrolments = eventId.HasValue
                    ? enrolments.Where(e => e.EventID == eventId.Value).ToList()
                    : enrolments;

                ViewBag.Enrolments = selectedEnrolments;

                var selectedResults = eventId.HasValue
                    ? results.Where(r => r.EventID == eventId.Value).ToList()
                    : results;

                return View(selectedResults);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve results. Please try again.";

                return View(new List<ResultViewModel>());
            }
        }

        // ORGANISER: CAPTURE RESULT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResultViewModel result)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            if (result.EnrolmentID <= 0 ||
                result.FinishTime <= TimeSpan.Zero ||
                result.FinishPosition <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid enrolment, finish time and position.";

                return RedirectToAction("Index",
                    new { eventId = result.EventID });
            }

            try
            {
                var success = await _apiService.CreateResultAsync(
                    result, token);

                TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                    success
                    ? "Result captured successfully."
                    : "Could not capture result. A result may already exist for this enrolment.";

            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to communicate with the RaceDay API.";
            }

            return RedirectToAction("Index",
                new { eventId = result.EventID });
        }

        // ORGANISER: PUBLISH RESULT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var success = await _apiService.PublishResultAsync(id, token);

                TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                    success
                    ? "Result published successfully."
                    : "Unable to publish result.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to communicate with the RaceDay API.";
            }

            return RedirectToAction("Index");
        }

        // PARTICIPANT: VIEW OWN PUBLISHED RESULTS
        public async Task<IActionResult> MyResults()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null || role != "Participant")
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var results = await _apiService.GetMyResultsAsync(token);
                return View(results);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve your results.";

                return View(new List<ResultViewModel>());
            }
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null
                && HttpContext.Session.GetString("Role") == "Organiser";
        }
    }
}
