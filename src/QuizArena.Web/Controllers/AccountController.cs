using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizArena.Business.Dtos;
using QuizArena.Business.Services;
using QuizArena.Web.Models;

namespace QuizArena.Web.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
    {
        _authService = authService;
    }

    // ---------- KAYIT ----------

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity!.IsAuthenticated)
            return RedirectToAction("Panel", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new RegisterDto
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            Role = model.Role,
            TeacherInviteCode = model.TeacherInviteCode
        };

        var result = await _authService.RegisterAsync(dto);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
            return View(model);
        }

        await SignInUser(result.Data!);
        TempData["Success"] = "Hoş geldin " + result.Data!.FullName + "! Hesabın oluşturuldu.";
        return RedirectToAction("Panel", "Home");
    }

    // ---------- GİRİŞ ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity!.IsAuthenticated)
            return RedirectToAction("Panel", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.LoginAsync(new LoginDto { Email = model.Email, Password = model.Password });
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
            return View(model);
        }

        await SignInUser(result.Data!);

        // Sadece kendi sitemizdeki bir adrese geri dön (başka siteye yönlendirme saldırısını önler)
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Panel", "Home");
    }

    // ---------- ÇIKIŞ ----------

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Çıkış yapıldı.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    // Kullanıcı bilgilerini (Id, ad, rol) çereze yazıyor
    private async Task SignInUser(UserDto user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()) // "Teacher" ya da "Student"
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
