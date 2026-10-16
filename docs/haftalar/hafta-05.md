# Hafta 5 — Kullanıcı Yönetimi ve Kimlik Doğrulama

**Amaç:** Kayıt, giriş ve rol tabanlı erişimle kullanıcı yönetimini kurmak.

## Yapılanlar

- [x] Kayıt/giriş akışı: `AccountController` + `AuthService`
- [x] Parola hash'leme: BCrypt (maliyet 11, otomatik tuz) → `Business/Helpers/PasswordHelper.cs`
- [x] Parola politikası: ≥ 8 karakter, büyük + küçük harf + rakam → `UserValidator`
- [x] Oturum yönetimi: imzalı, HttpOnly çerez; 2 saat, kayan süre (sliding) → `Program.cs`
- [x] Rol tabanlı erişim: `[Authorize(Roles = "Teacher")]` (QuizController), `[Authorize(Roles = "Student")]` (StudentController)
- [x] Öğretmen kaydı davet kodu ister (herkes kendini öğretmen yapamaz)
- [x] Başarısız giriş: 5 hatalı denemede hesap 5 dakika kilitlenir; hata mesajı e‑postanın kayıtlı olup olmadığını belli etmez
- [x] CSRF koruması (tüm POST'larda antiforgery token), açık yönlendirme koruması (`Url.IsLocalUrl`)
- [x] Kısa güvenlik notu → [`../guvenlik-notu.md`](../guvenlik-notu.md)

## Teslim edilecekler

| Çıktı | Nerede |
|---|---|
| Çalışan kayıt/giriş | `/Account/Register`, `/Account/Login` |
| Rol tabanlı erişim | `QuizController`, `StudentController`, `HomeController.Panel` |
| Kısa güvenlik notu | `docs/guvenlik-notu.md` |

## Gösterilecek ilerleme (demo)

1. Öğrenci olarak kayıt ol → öğrenci paneline yönlenir.
2. Zayıf parola dene (`abcdefgh`) → sunucu tarafı hata mesajları.
3. Öğretmen olarak kayıt olmayı yanlış davet koduyla dene → reddedilir.
4. DB Browser'da `Users` tablosunu aç → `PasswordHash` sütununda `$2a$11$...` (düz parola yok).
5. Öğrenci girişliyken adres çubuğuna `/Quiz` yaz → "Erişim engellendi".
6. Çıkış yap, `/Quiz` yaz → giriş sayfasına yönlenir (`?ReturnUrl=/Quiz`), giriş sonrası oraya döner.
7. 5 kez yanlış parola → hesap kilitlenir.

## Kontrol kriterleri

- [x] Parola düz metin saklanmıyor → sadece BCrypt özeti
- [x] Yetkisiz erişim engelleniyor → rol kontrolü sunucuda (`[Authorize]`) + sahiplik kontrolü iş katmanında
- [x] Oturum doğru yönetiliyor → süre, HttpOnly, SameSite, çıkışta çerez siliniyor

## Senin kendi geliştirmen gereken kısımlar

Hoca "parola hash ve doğrulama akışını kendin kur" diyor. Bu yüzden:
- `AuthService.LoginAsync` metodunu satır satır okuyup her adımı yorum satırıyla kendi cümlelerinle yeniden yaz.
- "Rol kontrolünü en az bir uçta uygula" → kendin yeni bir uç ekle: ör. öğretmene özel `Quiz/Stats` sayfası (`[Authorize(Roles = "Teacher")]`) ya da öğrenciye özel bir "Profilim" sayfası.
- Bir deneme yap: `[Authorize]` satırını geçici olarak sil, öğrenciyle `/Quiz`'e gir, ne olduğunu gör, sonra geri koy. Sözlüde anlatmak için iyi bir deneyim.

## Sözlü sınav soruları — cevap taslakları

**1) Parolayı neden hash'liyoruz, tuz (salt) nedir?**
Veritabanı çalınırsa parolalar okunamasın diye. Hash tek yönlüdür, geri çevrilemez; girişte kullanıcının yazdığı parolayı tekrar hash'leyip karşılaştırırız. Tuz, her parolaya eklenen rastgele bir değerdir: aynı parolayı kullanan iki kişinin özeti farklı çıkar ve hazır özet tabloları (rainbow table) işe yaramaz. BCrypt tuzu kendisi üretir ve özetin içinde saklar. Ayrıca kasıtlı olarak yavaştır (maliyet 11), bu da kaba kuvvet denemelerini pahalı yapar.

**2) Oturum ve token farkı nedir?**
Oturumda (session) kimlik bilgisi sunucuya bağlı tutulur; tarayıcı bir çerez taşır. Token'da (ör. JWT) kullanıcı bilgisi imzalı bir metnin içindedir; istemci her istekte `Authorization` başlığıyla gönderir, sunucu bir şey saklamaz. Ben sunucu tarafında sayfa üreten bir MVC uygulaması yaptığım için çerez tabanlı kimlik doğrulama seçtim; ASP.NET Core çerezi şifreleyip imzalıyor, HttpOnly olduğu için JavaScript erişemiyor. Mobil uygulama ya da ayrı bir API olsaydı JWT daha uygun olurdu. Çıkış yapınca çerez silinir; JWT'de ise token süresi dolana kadar geçerli kalır (iptal etmek zordur).

**3) Rol tabanlı erişimi nasıl uyguladın?**
Giriş yapınca rolü bir claim olarak çereze yazıyorum (`ClaimTypes.Role = "Teacher"`). Controller'larda `[Authorize(Roles = "Teacher")]` kullanıyorum; ASP.NET Core her istekte bu claim'i kontrol ediyor. Menüdeki linkleri de role göre gizliyorum ama bu sadece görüntü; asıl kontrol sunucuda. Ek olarak, bir öğretmenin başka öğretmenin quizine erişmesini `QuizService.CheckOwner` engelliyor (rol yetmez, sahiplik de gerekir).

**4) Başarısız giriş denemesinde ne oluyor?**
"E‑posta veya parola hatalı" mesajı dönüyor; hangisinin yanlış olduğunu söylemiyorum ki saldırgan kayıtlı e‑postaları tespit edemesin. Kullanıcının `FailedLoginCount` sayacı artıyor; 5'e ulaşınca hesap 5 dakika kilitleniyor (`LockoutEndUtc`). Başarılı girişte sayaç sıfırlanıyor.

**5) Yetkisiz kullanıcı korumalı sayfaya giderse ne olur?**
Giriş yapmamışsa giriş sayfasına yönlenir ve adres `ReturnUrl` olarak saklanır; girişten sonra oraya döner (sadece kendi sitemizdeki adreslere izin veriyorum, açık yönlendirme saldırısına karşı). Giriş yapmış ama rolü uygun değilse "Erişim engellendi" (403) sayfası görür. Başka öğretmenin quiz id'sini URL'ye yazarsa yine 403 alır.

## Teorik kavramlar (kısa)

- **Kimlik doğrulama (authentication):** "Sen kimsin?" · **Yetkilendirme (authorization):** "Buna iznin var mı?"
- **Hash + salt**, **BCrypt maliyet faktörü**
- **Çerez oturumu vs JWT**
- **OWASP Top 10** ilgili maddeler: Broken Access Control, Cryptographic Failures, Identification and Authentication Failures → güvenlik notunda
