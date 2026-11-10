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

    // GET: /Organiser
    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events/organiser");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();

        return View(events);
    }

    // GET: /Organiser/CreateEvent
    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        return View();
    }

    // POST: /Organiser/CreateEvent
    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventRequest model, IFormFile? bannerFile)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(model);

        // 1. Create the event
        var response = await _api.SendAsync(HttpMethod.Post, "api/events", model);

        if (!response.IsSuccessStatusCode)
        {
            string errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API Error: {errorMsg}");
            return View(model);
        }

        // 2. Upload the banner
        if (bannerFile != null && bannerFile.Length > 0)
        {
            var createdEvent = await _api.ReadAsync<EventDto>(response);
            if (createdEvent != null)
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(bannerFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(bannerFile.ContentType);
                content.Add(fileContent, "file", bannerFile.FileName);

                // Use the API client to ensure the cookie is sent!
                var fileResponse = await _api.SendMultipartAsync(HttpMethod.Post, $"api/events/{createdEvent.EventId}/banner", content);
                
                if (!fileResponse.IsSuccessStatusCode)
                {
                    string errorMsg = await fileResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Upload failed: {errorMsg} (Status: {fileResponse.StatusCode})";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Organiser/Categories/{eventId}
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

    // POST: /Organiser/CreateCategory
    [HttpPost]
    public async Task<IActionResult> CreateCategory(int eventId, string name, string categoryType)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var request = new 
        { 
            EventId = eventId, 
            Name = name, 
            CategoryType = categoryType 
        };

        var response = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", request);

        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    // GET: /Organiser/Enrolments/{eventId}
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

    // GET: /Organiser/Results/{eventId}
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

    // POST: /Organiser/CaptureResult
    [HttpPost]
    public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, string finishTime, int position)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        try
        {
            var timeSpan = TimeSpan.Parse(finishTime);

            var request = new
            {
                EnrolmentId = enrolmentId,
                FinishTime = timeSpan,
                FinishingPosition = position,
                OverallPosition = position,
                Status = "Finished"
            };

            var response = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/results", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Result saved successfully!";
            }
            else
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Message"] = $"Failed to save. API said: {errorMsg}";
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error: " + ex.Message;
        }

        return RedirectToAction(nameof(Results), new { id = eventId });
    }
}