
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        // Display the appropriate dashboard
        public IActionResult Index()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            // Redirect users who are not logged in
            if (userID == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Display the Organiser dashboard
            if (role == "Organiser")
            {
                return View("OrganiserDashboard");
            }

            // Display the Participant dashboard
            if (role == "Participant")
            {
                return View("ParticipantDashboard");
            }

            // Redirect users with an unrecognised role
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }
}
