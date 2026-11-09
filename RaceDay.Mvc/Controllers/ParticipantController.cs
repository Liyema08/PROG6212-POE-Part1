using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    private bool IsParticipant()
    {
        var role = HttpContext.Session.GetString("Role");
        return role == "Participant";
    }

    public async Task<IActionResult> Index()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();

        events = events.Where(e => e.EventDate >= DateTime.Today).ToList();

        return View(events);
    }

    public async Task<IActionResult> EventDetails(int id)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(response);

        if (ev == null) return NotFound();

        return View(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var request = new CreateEnrolmentRequest { EventId = eventId, CategoryId = categoryId };
        var response = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/enrol", request);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not enrol. You may already be enrolled in this event.";
            return RedirectToAction("EventDetails", new { id = eventId });
        }

        TempData["Success"] = "You have successfully entered the event!";
        return RedirectToAction("EventDetails", new { id = eventId });
    }

    public async Task<IActionResult> MyEvents()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/me");
        var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new();

        return View(enrolments);
    }

    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/results/me");
        var results = await _api.ReadAsync<List<ResultDto>>(response) ?? new();

        return View(results);
    }
}