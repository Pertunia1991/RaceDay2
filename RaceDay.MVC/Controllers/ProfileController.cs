using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ProfileController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        // DISPLAY THE USER PROFILE
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var profile = await _apiService.GetUserAsync(
                    userID.Value, token);

                if (profile == null)
                {
                    TempData["ErrorMessage"] =
                        "Unable to retrieve your profile.";

                    return RedirectToAction("Index", "Home");
                }

                return View(profile);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index", "Home");
            }
        }

        // UPDATE THE USER PROFILE AND UPLOAD PROFILE PICTURE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            UserProfileViewModel profile,
            IFormFile? profilePicture)
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            var token = HttpContext.Session.GetString("JWToken");

            if (userID == null || string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            if (userID.Value != profile.UserID)
                return Forbid();

            if (!ModelState.IsValid)
                return View(profile);

            try
            {
                // UPDATE PROFILE DETAILS
                var success = await _apiService.UpdateUserAsync(
                    userID.Value, profile, token);

                if (!success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to update your profile. Please check your details.");

                    return View(profile);
                }

                // UPLOAD PROFILE PICTURE IF ONE WAS SELECTED
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var allowedTypes = new[]
                    {
                        "image/jpeg",
                        "image/png",
                        "image/gif",
                        "image/webp"
                    };

                    if (!allowedTypes.Contains(
                        profilePicture.ContentType.ToLowerInvariant()))
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Only JPG, PNG, GIF and WEBP images are allowed.");

                        return View(profile);
                    }

                    if (profilePicture.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "The profile picture must not exceed 5 MB.");

                        return View(profile);
                    }

                    var imageUrl = await _apiService.UploadFileAsync(
                        $"api/Users/{userID.Value}/profile-picture",
                        profilePicture,
                        token);

                    if (string.IsNullOrWhiteSpace(imageUrl))
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Your profile details were saved, but the picture could not be uploaded.");

                        return View(profile);
                    }

                    profile.ProfilePictureURL = imageUrl;
                }

                HttpContext.Session.SetString("FirstName", profile.FirstName);
                HttpContext.Session.SetString("LastName", profile.LastName);
                HttpContext.Session.SetString("Email", profile.Email);

                TempData["SuccessMessage"] =
                    "Your profile has been updated successfully.";

                return RedirectToAction("Index");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the RaceDay API.");

                return View(profile);
            }
        }
    }
}