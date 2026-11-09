
using Microsoft.AspNetCore.Http;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;

        public RaceDayApiService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("RaceDayAPI");
        }

        // LOGIN
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        // REGISTER
        public async Task<(bool Success, string Message)> RegisterAsync(
            RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/register", request);

            if (response.IsSuccessStatusCode)
                return (true, "Registration successful.");

            var errorMessage = await response.Content.ReadAsStringAsync();

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Conflict)
            {
                return (false,
                    "An account with this email already exists.");
            }

            return (false, string.IsNullOrWhiteSpace(errorMessage)
                ? "Registration could not be completed."
                : errorMessage);
        }

        // CREATE AN AUTHENTICATED API REQUEST
        private HttpRequestMessage CreateAuthenticatedRequest(
            HttpMethod method,
            string url,
            string token)
        {
            var request = new HttpRequestMessage(method, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return request;
        }

        // GET ALL EVENTS
        public async Task<List<EventViewModel>> GetEventsAsync(string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, "api/Events", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<EventViewModel>>()
                ?? new List<EventViewModel>();
        }

        // GET ONE EVENT
        public async Task<EventViewModel?> GetEventAsync(
            int id, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, $"api/Events/{id}", token);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<EventViewModel>();
        }

        // CREATE EVENT
        public async Task<bool> CreateEventAsync(
            EventViewModel eventItem, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Post, "api/Events", token);

            request.Content = JsonContent.Create(eventItem);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // UPDATE EVENT
        public async Task<bool> UpdateEventAsync(
            int id, EventViewModel eventItem, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Put, $"api/Events/{id}", token);

            request.Content = JsonContent.Create(eventItem);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // DELETE EVENT
        public async Task<bool> DeleteEventAsync(int id, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Delete, $"api/Events/{id}", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // GET ALL EVENT TYPES
        public async Task<List<EventTypeViewModel>> GetEventTypesAsync()
        {
            using var response = await _httpClient.GetAsync("api/EventTypes");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<EventTypeViewModel>>()
                ?? new List<EventTypeViewModel>();
        }

        // GET MY ENROLMENTS - PARTICIPANT
        public async Task<List<EnrolmentViewModel>> GetMyEnrolmentsAsync(
            string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, "api/Enrolments/my", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<EnrolmentViewModel>>()
                ?? new List<EnrolmentViewModel>();
        }

        // GET ALL ENROLMENTS - ORGANISER
        public async Task<List<OrganiserEnrolmentViewModel>> GetAllEnrolmentsAsync(
            string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, "api/Enrolments", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<
                List<OrganiserEnrolmentViewModel>>()
                ?? new List<OrganiserEnrolmentViewModel>();
        }

        // GET CATEGORIES FOR A SPECIFIC EVENT
        public async Task<List<CategoryViewModel>> GetCategoriesByEventAsync(
            int eventId, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get,
                $"api/Categories/event/{eventId}",
                token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>()
                ?? new List<CategoryViewModel>();
        }

        // CREATE CATEGORY
        public async Task<bool> CreateCategoryAsync(
            CategoryViewModel category, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Post, "api/Categories", token);

            request.Content = JsonContent.Create(category);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // UPDATE CATEGORY
        public async Task<bool> UpdateCategoryAsync(
            int id, CategoryViewModel category, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Put, $"api/Categories/{id}", token);

            request.Content = JsonContent.Create(category);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // DELETE CATEGORY
        public async Task<bool> DeleteCategoryAsync(
            int id, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Delete, $"api/Categories/{id}", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // CREATE PARTICIPANT ENROLMENT
        public async Task<bool> CreateEnrolmentAsync(
            EnrolmentRequest requestModel, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Post, "api/Enrolments", token);

            request.Content = JsonContent.Create(requestModel);

            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Conflict)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }

        // GET ALL RESULTS - ORGANISER
        public async Task<List<ResultViewModel>> GetAllResultsAsync(
            string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, "api/Results", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ResultViewModel>>()
                ?? new List<ResultViewModel>();
        }

        // GET RESULTS FOR ONE EVENT - ORGANISER
        public async Task<List<ResultViewModel>> GetEventResultsAsync(
            int eventId, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, $"api/Results/event/{eventId}", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ResultViewModel>>()
                ?? new List<ResultViewModel>();
        }

        // CAPTURE A RESULT - ORGANISER
        public async Task<bool> CreateResultAsync(
            ResultViewModel result, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Post, "api/Results", token);

            request.Content = JsonContent.Create(result);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // UPDATE A RESULT - ORGANISER
        public async Task<bool> UpdateResultAsync(
            int id, ResultViewModel result, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Put, $"api/Results/{id}", token);

            request.Content = JsonContent.Create(result);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // PUBLISH A RESULT - ORGANISER
        public async Task<bool> PublishResultAsync(
            int id, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Put, $"api/Results/{id}/publish", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // GET MY PUBLISHED RESULTS - PARTICIPANT
        public async Task<List<ResultViewModel>> GetMyResultsAsync(
            string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, "api/Results/my", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ResultViewModel>>()
                ?? new List<ResultViewModel>();
        }

        // DELETE A RESULT - ORGANISER
        public async Task<bool> DeleteResultAsync(
            int id, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Delete, $"api/Results/{id}", token);

            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        // GET A USER PROFILE
        public async Task<UserProfileViewModel?> GetUserAsync(int id, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Get, $"api/Users/{id}", token);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserProfileViewModel>();
        }

        // UPDATE A USER PROFILE
        public async Task<bool> UpdateUserAsync(
            int id, UserProfileViewModel user, string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Put, $"api/Users/{id}", token);

            request.Content = JsonContent.Create(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.ProfilePictureURL
            });

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<string?> UploadFileAsync(
            string url,
            IFormFile file,
            string token)
        {
            using var request = CreateAuthenticatedRequest(
                HttpMethod.Post,
                url,
                token);

            using var form = new MultipartFormDataContent();

            await using var stream = file.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "file", file.FileName);

            request.Content = form;

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var responseStream =
                await response.Content.ReadAsStreamAsync();

            using var document =
                await JsonDocument.ParseAsync(responseStream);

            if (document.RootElement.TryGetProperty(
                "imageUrl", out var imageUrl))
            {
                return imageUrl.GetString();
            }

            return null;
        }

    }
}
