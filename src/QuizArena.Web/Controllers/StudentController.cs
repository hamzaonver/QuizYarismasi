using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizArena.Business.Common;
using QuizArena.Business.Services;
using QuizArena.Web.Models;

namespace QuizArena.Web.Controllers;

// Öğrenci paneli ve canlı oturuma katılım (Hafta 7). Sadece öğrenciler girebilir.
[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly SessionService _sessionService;

    public StudentController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    private int StudentId
    {
        get { return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); }
    }

    public async Task<IActionResult> Index()
    {
        var model = new StudentPanelViewModel
        {
            MySessions = await _sessionService.GetStudentSessionsAsync(StudentId)
        };

        // Takma ad kutusu öğrencinin adıyla dolu gelsin
        string name = User.Identity!.Name ?? "";
        model.Join.Nickname = name.Length > 20 ? name.Substring(0, 20) : name;

        return View(model);
    }

    // Katılım kodu formu buraya gönderilir
    [HttpPost]
    public async Task<IActionResult> Join(StudentPanelViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.MySessions = await _sessionService.GetStudentSessionsAsync(StudentId);
            return View("Index", model);
        }

        var result = await _sessionService.JoinAsync(model.Join.ToDto(), StudentId);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
            model.MySessions = await _sessionService.GetStudentSessionsAsync(StudentId);
            return View("Index", model);
        }

        return RedirectToAction("Play", new { id = result.Data });
    }

    // Öğrencinin oturum ekranı (bekleme salonu / mevcut soru / bitti)
    public async Task<IActionResult> Play(int id)
    {
        var result = await _sessionService.GetStudentViewAsync(id, StudentId);
        if (!result.Success)
        {
            if (result.ErrorType == ErrorType.NotFound)
                return NotFound();
            return Forbid();
        }

        return View(result.Data);
    }

    // Sayfa 3 saniyede bir bunu çağırıp oturum durumu değişti mi bakıyor
    [HttpGet]
    public async Task<IActionResult> PlayState(int id)
    {
        var result = await _sessionService.GetStudentViewAsync(id, StudentId);
        if (!result.Success)
            return NotFound();

        var s = result.Data!;
        return Json(new
        {
            status = s.Status.ToString(),
            currentQuestionIndex = s.CurrentQuestionIndex,
            participantCount = s.ParticipantCount
        });
    }
}
