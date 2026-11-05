using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizArena.Business.Common;
using QuizArena.Business.Services;

namespace QuizArena.Web.Controllers;

// Canlı oturum - öğretmen tarafı (Hafta 7). Sadece öğretmenler girebilir.
[Authorize(Roles = "Teacher")]
public class SessionController : Controller
{
    private readonly SessionService _sessionService;

    public SessionController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    private int TeacherId
    {
        get { return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); }
    }

    // Öğretmenin açtığı oturumların listesi
    public async Task<IActionResult> Index()
    {
        var sessions = await _sessionService.GetTeacherSessionsAsync(TeacherId);
        return View(sessions);
    }

    // Quiz sayfasındaki "Canlı oturum başlat" butonu buraya gelir
    [HttpPost]
    public async Task<IActionResult> Create(int quizId)
    {
        var result = await _sessionService.CreateAsync(quizId, TeacherId);
        if (!result.Success)
        {
            if (result.ErrorType == ErrorType.NotFound)
                return NotFound();
            if (result.ErrorType == ErrorType.Forbidden)
                return Forbid();

            TempData["Error"] = string.Join(" ", result.Errors);
            return RedirectToAction("Details", "Quiz", new { id = quizId });
        }

        return RedirectToAction("Host", new { id = result.Data });
    }

    // Öğretmenin oturum ekranı (katılım kodu, katılımcılar, mevcut soru)
    public async Task<IActionResult> Host(int id)
    {
        var result = await _sessionService.GetHostViewAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result);

        return View(result.Data);
    }

    // Sayfa 3 saniyede bir bunu çağırıp yeni katılımcı var mı, durum değişti mi bakıyor
    [HttpGet]
    public async Task<IActionResult> State(int id)
    {
        var result = await _sessionService.GetHostViewAsync(id, TeacherId);
        if (!result.Success)
            return NotFound();

        var s = result.Data!;
        return Json(new
        {
            status = s.Status.ToString(),
            currentQuestionIndex = s.CurrentQuestionIndex,
            participants = s.Participants.Select(p => p.Nickname).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        var result = await _sessionService.StartAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, id);

        TempData["Success"] = "Oturum başladı!";
        return RedirectToAction("Host", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Next(int id)
    {
        var result = await _sessionService.NextQuestionAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, id);

        return RedirectToAction("Host", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Finish(int id)
    {
        var result = await _sessionService.FinishAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, id);

        TempData["Success"] = "Oturum bitirildi.";
        return RedirectToAction("Host", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _sessionService.CancelAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, id);

        TempData["Success"] = "Oturum iptal edildi.";
        return RedirectToAction("Details", "Quiz", new { id = result.Data });
    }

    // Kayıt yoksa 404, başkasınınsa 403, diğer hatalarda mesajla oturum sayfasına dön
    private IActionResult ShowError(Result result, int? sessionId = null)
    {
        if (result.ErrorType == ErrorType.NotFound)
            return NotFound();
        if (result.ErrorType == ErrorType.Forbidden)
            return Forbid();

        TempData["Error"] = string.Join(" ", result.Errors);
        if (sessionId != null)
            return RedirectToAction("Host", new { id = sessionId });
        return RedirectToAction("Index");
    }
}
