using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizArena.Web.Models;

namespace QuizArena.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity!.IsAuthenticated)
            return RedirectToAction("Panel");

        return View();
    }

    // Girişten sonra kullanıcıyı rolüne göre kendi sayfasına gönderir
    [Authorize]
    public IActionResult Panel()
    {
        if (User.IsInRole("Teacher"))
            return RedirectToAction("Index", "Quiz");

        return RedirectToAction("Index", "Student");
    }

    public IActionResult StatusCodePage(int code)
    {
        ViewBag.Code = code;
        return View();
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
