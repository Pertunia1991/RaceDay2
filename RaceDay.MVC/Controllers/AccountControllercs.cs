
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            RaceDayApiService apiService,
            ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var result = await _apiService.LoginAsync(request);

                if (result == null ||
                    string.IsNullOrWhiteSpace(result.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid email address or password.");

                    return View(request);
                }

                HttpContext.Session.SetInt32("UserID", result.UserID);
                HttpContext.Session.SetString("FirstName", result.FirstName);
                HttpContext.Session.SetString("LastName", result.LastName);
                HttpContext.Session.SetString("Email", result.Email);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("JWToken", result.Token);

                TempData["SuccessMessage"] =
                    "Welcome to RaceDay, " + result.FirstName + "!";

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API login connection failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to RaceDay API. Please try again.");

                return View(request);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var result = await _apiService.RegisterAsync(request);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(request);
                }

                TempData["SuccessMessage"] =
                    "Your Participant account has been created. Please log in.";

                return RedirectToAction("Login");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API registration connection failed.");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to RaceDay API. Please try again.");

                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
