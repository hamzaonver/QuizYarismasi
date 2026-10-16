# Kısa Güvenlik Notu — QuizArena (Hafta 5)

## Alınan önlemler

| Tehdit (OWASP Top 10 2021) | Önlem | Kod |
|---|---|---|
| **A07 Kimlik doğrulama hataları** — parola tahmini, kaba kuvvet | Parola politikası (≥8, büyük/küçük harf, rakam); 5 hatalı denemede 5 dk kilit; genel hata mesajı ("E‑posta veya parola hatalı") | `UserValidator`, `AuthService.LoginAsync` |
| **A02 Kriptografik hatalar** — parola sızıntısı | Parolalar BCrypt (maliyet 11, rastgele tuz) ile özetlenir, düz metin hiçbir yerde tutulmaz/loglanmaz | `Helpers/PasswordHelper.cs` |
| **A01 Erişim kontrolü** — yetkisiz sayfa/kayıt erişimi | Sunucu tarafı rol kontrolü `[Authorize(Roles=…)]`; her quiz/soru işleminde sahiplik kontrolü (IDOR'a karşı); `TeacherId` formdan değil oturumdan alınır | `QuizController`, `QuizService.CheckOwner` |
| Yetki yükseltme — herkesin öğretmen olması | Öğretmen kaydı davet kodu ister; kod yapılandırmadan gelir, boşsa öğretmen kaydı kapalı | `AuthService.RegisterAsync`, `AuthSettings` |
| Oturum çalınması | Çerez: HttpOnly (JS okuyamaz), ASP.NET Core tarafından şifreli ve imzalı; 2 saat süre, kullandıkça yenilenir | `Program.cs` |
| CSRF (siteler arası istek sahteciliği) | Tüm POST isteklerinde antiforgery token zorunlu (`AutoValidateAntiforgeryTokenAttribute`) — token'sız istek 400 alır | `Program.cs` |
| Açık yönlendirme (open redirect) | Giriş sonrası `ReturnUrl` sadece yerel adresse kullanılır (`Url.IsLocalUrl`) | `AccountController.Login` |
| **A03 Enjeksiyon** — SQL / XSS | EF Core parametreli sorgular üretir (elle SQL birleştirme yok); Razor çıktıyı otomatik HTML‑encode eder | Tüm repository'ler ve view'lar |
| Kullanıcı tespiti (user enumeration) | E‑posta yanlış da olsa parola yanlış da olsa aynı mesaj gösterilir ("E‑posta veya parola hatalı") | `AuthService.LoginAsync` |
| **A05 Güvenlik yapılandırması** — gizli bilgi sızıntısı | `appsettings.json` içinde gizli bilgi yok; geliştirme davet kodu sadece `appsettings.Development.json`'da; üretimde ortam değişkeni `Auth__TeacherInviteCode`. `.env`, `*.db` .gitignore'da | `.gitignore`, `appsettings*.json` |
| KVKK | Örnek veriler tamamen sentetik ("Demo …", `@quizarena.test`) | `DbInitializer` |

## Bilinen sınırlamalar / sonraki adımlar

- **HTTPS:** Yayına alırken çerez için `Cookie.SecurePolicy = Always` ayarlanmalı ve HTTPS zorunlu olmalı (Hafta 14).
- **Rol değişince oturum:** Çerezdeki rol, oturum süresi dolana kadar geçerli. Rol/şifre değişikliğinde oturumu düşürmek için "security stamp" kontrolü eklenebilir.
- **Kilitleme ile hizmet engelleme:** Saldırgan başkasının e‑postasıyla bilerek 5 hatalı deneme yapıp hesabı kilitleyebilir. İleride IP bazlı hız sınırı (rate limiting, .NET 8'de hazır `AddRateLimiter`) eklenebilir.
- **Parola sıfırlama** henüz yok (e‑posta servisi gerektirir).
- Misafir katılımı (Hafta 7) için takma ad filtresi ve oturum kodu tahmin edilemezliği ayrıca değerlendirilecek.
