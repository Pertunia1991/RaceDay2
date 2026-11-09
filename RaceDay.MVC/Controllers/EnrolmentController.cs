
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public EnrolmentsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // Display the participant's enrolments
        public async Task<IActionResult> Index()
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

            try
            {
                var enrolments =
                    await _apiService.GetMyEnrolmentsAsync(token);

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve your enrolments. Please try again.";

                return View(new List<EnrolmentViewModel>());
            }
        }
    }
}
