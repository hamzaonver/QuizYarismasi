using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizArena.Business;
using QuizArena.Business.Helpers;
using QuizArena.Business.Services;
using QuizArena.Data;
using QuizArena.Data.Repositories;
using QuizArena.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

// ---------- Ayarlar (appsettings.json) ----------
string connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=quizarena.db";
var authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>() ?? new AuthSettings();

// ---------- Veritabanı ----------
// SQL Server kullanmak istersem UseSqlite yerine UseSqlServer yazmam yeterli
builder.Services.AddDbContext<QuizArenaDbContext>(options => options.UseSqlite(connectionString));

// ---------- Repository'ler (Veri katmanı) ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>(); // Hafta 7

// ---------- Servisler (İş katmanı) ----------
builder.Services.AddSingleton(authSettings);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<SessionService>(); // Hafta 7

builder.Services.AddControllersWithViews(options =>
{
    // Bütün POST isteklerinde CSRF token kontrolü yap
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ---------- Giriş sistemi (çerez ile oturum) ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";               // giriş yapmamışsa buraya
        options.AccessDeniedPath = "/Account/AccessDenied"; // yetkisi yoksa buraya
        options.ExpireTimeSpan = TimeSpan.FromHours(2);     // oturum süresi
        options.SlidingExpiration = true;                   // kullandıkça süre uzar
        options.Cookie.HttpOnly = true;                     // JavaScript çereze erişemez
        options.Cookie.Name = "QuizArena.Auth";
    });

var app = builder.Build();

// ---------- Veritabanını hazırla ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuizArenaDbContext>();
    bool seedDemoData = app.Configuration.GetValue<bool>("Database:SeedDemoData");
    await DbInitializer.InitializeAsync(db, PasswordHelper.HashPassword, seedDemoData);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}"); // 404 için özel sayfa
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // önce kim olduğunu bul
app.UseAuthorization();  // sonra yetkisi var mı bak

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
