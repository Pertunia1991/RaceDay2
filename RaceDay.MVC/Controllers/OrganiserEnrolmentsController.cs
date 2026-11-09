
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserEnrolmentsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public OrganiserEnrolmentsController(
            RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY ALL PARTICIPANT ENROLMENTS
        public async Task<IActionResult> Index()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null || role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var enrolments =
                    await _apiService.GetAllEnrolmentsAsync(token);

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve participant enrolments.";

                return View(new List<Models.OrganiserEnrolmentViewModel>());
            }
        }
    }
}
