
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly RaceDayApiService _apiService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            RaceDayApiService apiService,
            ILogger<CategoriesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Display categories for a selected event
        public async Task<IActionResult> Index(int? eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                var events = await _apiService.GetEventsAsync(token!);

                ViewBag.Events = events;

                if (!events.Any())
                {
                    ViewBag.SelectedEventID = null;
                    return View(new List<CategoryViewModel>());
                }

                int selectedEventId = eventId ?? events.First().EventID;

                ViewBag.SelectedEventID = selectedEventId;

                var categories = await _apiService.GetCategoriesByEventAsync(
                    selectedEventId, token!);

                return View(categories);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to load event categories.");
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return View(new List<CategoryViewModel>());
            }
        }

        // Display the create category form
        [HttpGet]
        public IActionResult Create(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var model = new CategoryViewModel
            {
                EventID = eventId
            };

            return View(model);
        }

        // Save a new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                await _apiService.CreateCategoryAsync(model, token!);

                TempData["SuccessMessage"] =
                    "Category created successfully.";

                return RedirectToAction("Index",
                    new { eventId = model.EventID });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to create category.");
                ModelState.AddModelError(string.Empty,
                    "Unable to create the category. Please try again.");

                return View(model);
            }
        }

        // Display the edit category form
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                var categories = await _apiService.GetCategoriesByEventAsync(
                    eventId, token!);

                var category = categories.FirstOrDefault(
                    c => c.CategoryID == id);

                if (category == null)
                    return NotFound();

                return View(category);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to load category.");
                TempData["ErrorMessage"] =
                    "Unable to load the category.";

                return RedirectToAction("Index",
                    new { eventId });
            }
        }

        // Save category changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (id != model.CategoryID)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                await _apiService.UpdateCategoryAsync(
                    id, model, token!);

                TempData["SuccessMessage"] =
                    "Category updated successfully.";

                return RedirectToAction("Index",
                    new { eventId = model.EventID });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to update category.");
                ModelState.AddModelError(string.Empty,
                    "Unable to update the category. Please try again.");

                return View(model);
            }
        }

        // Delete a category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                await _apiService.DeleteCategoryAsync(id, token!);

                TempData["SuccessMessage"] =
                    "Category deleted successfully.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to delete category.");
                TempData["ErrorMessage"] =
                    "Unable to delete this category. It may have existing enrolments.";
            }

            return RedirectToAction("Index", new { eventId });
        }

        // Check whether the logged-in user is an Organiser
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser"
                && HttpContext.Session.GetString("JWToken") != null;
        }
    }
}
