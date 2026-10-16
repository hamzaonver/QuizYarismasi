using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizArena.Business.Common;
using QuizArena.Business.Services;
using QuizArena.Business.Validation;
using QuizArena.Web.Models;

namespace QuizArena.Web.Controllers;

// Quiz Oluşturma sayfaları. Sadece öğretmenler girebilir.
[Authorize(Roles = "Teacher")]
public class QuizController : Controller
{
    private readonly QuizService _quizService;

    public QuizController(QuizService quizService)
    {
        _quizService = quizService;
    }

    // Giriş yapan öğretmenin Id'si (çerezden okunuyor)
    private int TeacherId
    {
        get { return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); }
    }

    // ==================== QUIZ ====================

    public async Task<IActionResult> Index()
    {
        var quizzes = await _quizService.GetTeacherQuizzesAsync(TeacherId);
        return View(quizzes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _quizService.GetDetailAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result);

        return View(result.Data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("QuizForm", new QuizFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(QuizFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("QuizForm", model);

        var result = await _quizService.CreateAsync(TeacherId, model.ToDto());
        if (!result.Success)
        {
            AddErrors(result);
            return View("QuizForm", model);
        }

        TempData["Success"] = "Quiz oluşturuldu. Şimdi soru ekleyebilirsiniz.";
        return RedirectToAction("Details", new { id = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _quizService.GetDetailAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result);

        var model = new QuizFormViewModel
        {
            Id = result.Data!.Id,
            Title = result.Data.Title,
            Description = result.Data.Description,
            DefaultTimeLimitSeconds = result.Data.DefaultTimeLimitSeconds
        };
        return View("QuizForm", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, QuizFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
            return View("QuizForm", model);

        var result = await _quizService.UpdateAsync(id, TeacherId, model.ToDto());
        if (!result.Success)
        {
            if (result.ErrorType != ErrorType.Validation)
                return ShowError(result);
            AddErrors(result);
            return View("QuizForm", model);
        }

        TempData["Success"] = "Quiz bilgileri güncellendi.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _quizService.DeleteAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, id);

        TempData["Success"] = "Quiz silindi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id, bool publish)
    {
        var result = await _quizService.SetPublishedAsync(id, TeacherId, publish);
        if (!result.Success)
            return ShowError(result, id);

        TempData["Success"] = publish ? "Quiz yayınlandı." : "Quiz yayından kaldırıldı.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(int id)
    {
        var result = await _quizService.DuplicateAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, id);

        TempData["Success"] = "Quizin kopyası oluşturuldu.";
        return RedirectToAction("Details", new { id = result.Data });
    }

    // ==================== SORU ====================

    [HttpGet]
    public async Task<IActionResult> AddQuestion(int quizId)
    {
        var quiz = await _quizService.GetDetailAsync(quizId, TeacherId);
        if (!quiz.Success)
            return ShowError(quiz);

        if (quiz.Data!.IsLocked)
        {
            TempData["Error"] = QuizRules.LockedMessage;
            return RedirectToAction("Details", new { id = quizId });
        }

        var model = new QuestionFormViewModel
        {
            QuizId = quizId,
            QuizTitle = quiz.Data.Title,
            TimeLimitSeconds = quiz.Data.DefaultTimeLimitSeconds
        };
        model.FillEmptyOptions();
        return View("QuestionForm", model);
    }

    [HttpPost]
    public async Task<IActionResult> AddQuestion(int quizId, QuestionFormViewModel model)
    {
        model.QuizId = quizId;
        model.FillEmptyOptions();
        if (!ModelState.IsValid)
            return View("QuestionForm", model);

        var result = await _quizService.AddQuestionAsync(quizId, TeacherId, model.ToDto());
        if (!result.Success)
        {
            if (result.ErrorType != ErrorType.Validation)
                return ShowError(result);
            AddErrors(result);
            return View("QuestionForm", model);
        }

        TempData["Success"] = "Soru eklendi.";
        return RedirectToAction("Details", new { id = quizId });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int id)
    {
        var question = await _quizService.GetQuestionAsync(id, TeacherId);
        if (!question.Success)
            return ShowError(question);

        var quiz = await _quizService.GetDetailAsync(question.Data!.QuizId, TeacherId);
        if (quiz.Data!.IsLocked)
        {
            TempData["Error"] = QuizRules.LockedMessage;
            return RedirectToAction("Details", new { id = quiz.Data.Id });
        }

        var model = QuestionFormViewModel.FromDto(question.Data, quiz.Data.Title);
        return View("QuestionForm", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditQuestion(int id, QuestionFormViewModel model)
    {
        model.QuestionId = id;
        model.FillEmptyOptions();
        if (!ModelState.IsValid)
            return View("QuestionForm", model);

        var result = await _quizService.UpdateQuestionAsync(id, TeacherId, model.ToDto());
        if (!result.Success)
        {
            if (result.ErrorType != ErrorType.Validation)
                return ShowError(result);
            AddErrors(result);
            return View("QuestionForm", model);
        }

        TempData["Success"] = "Soru güncellendi.";
        return RedirectToAction("Details", new { id = model.QuizId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuestion(int id, int quizId)
    {
        var result = await _quizService.DeleteQuestionAsync(id, TeacherId);
        if (!result.Success)
            return ShowError(result, quizId);

        TempData["Success"] = "Soru silindi.";
        return RedirectToAction("Details", new { id = quizId });
    }

    [HttpPost]
    public async Task<IActionResult> MoveQuestion(int id, int quizId, int direction)
    {
        var result = await _quizService.MoveQuestionAsync(id, TeacherId, direction);
        if (!result.Success)
            return ShowError(result, quizId);

        return RedirectToAction("Details", new { id = quizId });
    }

    // ==================== HATA GÖSTERME ====================

    // Kayıt yoksa 404, başkasınınsa 403, diğer hatalarda mesajla quiz sayfasına dön
    private IActionResult ShowError(Result result, int? quizId = null)
    {
        if (result.ErrorType == ErrorType.NotFound)
            return NotFound();

        if (result.ErrorType == ErrorType.Forbidden)
            return Forbid();

        TempData["Error"] = string.Join(" ", result.Errors);
        if (quizId != null)
            return RedirectToAction("Details", new { id = quizId });

        return RedirectToAction("Index");
    }

    private void AddErrors(Result result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);
    }
}
