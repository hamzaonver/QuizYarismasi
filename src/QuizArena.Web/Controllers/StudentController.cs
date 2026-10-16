using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuizArena.Web.Controllers;

// Öğrenci paneli. Oturuma katılma kısmı Hafta 7'de yapılacak.
[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
