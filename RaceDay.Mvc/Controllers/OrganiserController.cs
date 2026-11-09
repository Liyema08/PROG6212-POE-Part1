using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    private bool IsOrganiser()
    {
        var role = HttpContext.Session.GetString("Role");
        return role == "Organiser";
    }

    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events/organiser");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();

        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventRequest model, IFormFile? bannerFile)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(model);

        var response = await _api.SendAsync(HttpMethod.Post, "api/events", model);

        if (!response.IsSuccessStatusCode)
        {
            string errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API Error: {errorMsg}");
            return View(model);
        }

        if (bannerFile != null && bannerFile.Length > 0)
        {
            var createdEvent = await _api.ReadAsync<EventDto>(response);
            if (createdEvent != null)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5194");
                
                var apiCookie = HttpContext.Session.GetString("ApiSessionCookie");
                if (!string.IsNullOrEmpty(apiCookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", apiCookie);
                }

                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(bannerFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(bannerFile.ContentType);
                content.Add(fileContent, "file", bannerFile.FileName);

                await client.PostAsync($"api/events/{createdEvent.EventId}/banner", content);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Categories(int id)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
        var categories = await _api.ReadAsync<List<CategoryDto>>(response) ?? new();

        var eventResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(eventResponse);

        ViewBag.EventName = ev?.Name ?? "Event";
        ViewBag.EventId = id;

        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(int eventId, string name, string categoryType)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var request = new { EventId = eventId, Name = name, CategoryType = categoryType };
        await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", request);

        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    [HttpGet]
    public async Task<IActionResult> Enrolments(int id)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/enrolments");
        var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new();

        var eventResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(eventResponse);

        ViewBag.EventName = ev?.Name ?? "Event";
        ViewBag.EventId = id;

        return View(enrolments);
    }

    [HttpGet]
    public async Task<IActionResult> Results(int id)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/enrolments");
        var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new();

        var eventResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(eventResponse);

        ViewBag.EventName = ev?.Name ?? "Event";
        ViewBag.EventId = id;

        return View(enrolments);
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, string finishTime, int position)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        try
        {
            var timeSpan = TimeSpan.Parse(finishTime);
            var request = new { EnrolmentId = enrolmentId, FinishTime = timeSpan, FinishingPosition = position, OverallPosition = position, Status = "Finished" };

            var response = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/results", request);

            TempData["Message"] = response.IsSuccessStatusCode ? "Result saved successfully!" : "Failed to save result.";
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error: " + ex.Message;
        }

        return RedirectToAction(nameof(Results), new { id = eventId });
    }
}