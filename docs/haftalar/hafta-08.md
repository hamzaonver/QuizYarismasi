# Hafta 8 — VİZE: Ara Sürüm, Entegrasyon ve Sözlü Hazırlık

**Amaç:** O ana kadarki modülleri uçtan uca entegre edip çalışan bir ara sürüm ortaya koymak ve vize sözlüsüne hazırlanmak.

**Ara sürüm:** `v0.8.0` (GitHub'da Git tag ile etiketli) · Sürüm geçmişi: [CHANGELOG.md](../../CHANGELOG.md)

## Yapılan teknik çalışmalar

| PDF'teki görev | Ne yaptım | Nerede |
|---|---|---|
| Modüllerin uçtan uca entegrasyonu | Kullanıcı (H5) + veritabanı (H4) + quiz (H6) + canlı oturum (H7) birlikte çalışıyor. Bunu otomatik **entegrasyon testi** ile kanıtladım (38 kontrol) | `scripts/entegrasyon-testi.bat` |
| Bilinen hataların giderilmesi | 4 hata düzeltildi (aşağıdaki tablo) | `QuizService.cs`, `SessionService.cs`, `Home/Index.cshtml`, `_Layout.cshtml` |
| Ara sürüm etiketleme (Git tag) | `v0.8.0` etiketi ve GitHub Release oluşturuldu; sürüm numarası sitenin altında görünüyor | GitHub → Releases · `QuizArena.Web.csproj` → `<Version>` · `AppInfo.cs` |
| Kısa demo senaryosu | 7–8 dakikalık vize demosu, adım adım | [docs/demo-senaryosu.md](../demo-senaryosu.md) |
| Demo linki | `scripts\demo-link.bat` siteyi açıp internetten erişilebilen geçici bir link verir (SSH tüneli, hesap gerekmez). Kalıcı yayın Hafta 14'te | `scripts/demo-link.ps1`, `Program.cs` → `UseForwardedHeaders` |

## Düzeltilen bilinen hatalar

| # | Hata | Neden önemli | Çözüm | Dosya |
|---|---|---|---|---|
| 1 | Açık oturumu olan quiz **yayından kaldırılabiliyordu** | Öğrenciler bekleme salonunda taslak bir quizle kalıyordu (Hafta 6 ve 7 arasındaki entegrasyon hatası) | Yayından kaldırmadan önce açık oturum var mı diye bakılıyor; varsa "Önce oturumu bitirin" mesajı | `QuizService.SetPublishedAsync`, `QuizRepository.HasOpenSessionAsync` |
| 2 | İki öğrenci **aynı anda aynı takma adla** katılırsa ikincisi **hata sayfası** görüyordu | Kontrolü ikisi de aynı anda geçebiliyordu; veritabanındaki UNIQUE kural ikinciyi durdurunca hata çıkıyordu | Veritabanı hatası yakalanıyor, "Bu takma ad az önce alındı" mesajı gösteriliyor | `SessionService.JoinAsync` (`try / catch DbUpdateException`) |
| 3 | Ana sayfada "Canlı oturum" hâlâ **"Yakında"** yazıyordu | Hafta 7'de tamamlanmıştı, yanlış bilgi veriyordu | Metin güncellendi | `Views/Home/Index.cshtml` |
| 4 | Telefonda üst menü (hamburger) **ikonu görünmüyordu** | Demo linkine telefondan girenler menüyü bulamazdı | Menüye `navbar-dark` sınıfı eklendi | `_Layout.cshtml`, `site.css` |

## Entegrasyon testi (Temel entegrasyon testi yaklaşımı)

**Fikir:** Modülleri tek tek değil, **gerçek bir kullanıcının yaptığı sırayla birlikte** denemek. Test, siteyi gerçekten açıp tarayıcı gibi istek gönderiyor.

**Çalıştırma:** `scripts\entegrasyon-testi.bat` dosyasına çift tıkla → sonunda `SONUÇ: TÜM KONTROLLER BAŞARILI (38 kontrol)`

| Bölüm | Kontrol edilenler |
|---|---|
| Derleme + site | Proje hatasız derleniyor, site açılıyor |
| Hafta 4 · Veritabanı | Boş veritabanı migration ile oluşuyor, sentetik veri yükleniyor |
| Hafta 5 · Kullanıcı | Kayıt, aynı e-posta engeli, zayıf parola engeli, davet kodu, yanlış parola, rol kontrolü, girişsiz erişim engeli |
| Hafta 6 · Quiz | Quiz oluştur, boş quiz yayınlanamaz, tek şıklı soru reddedilir, 2 soru ekle, yayınla, başka öğretmen göremez |
| Hafta 7 · Oturum | Kod üretimi, kimse yokken başlamaz, yanlış kod, küçük harfli kod, aynı takma ad, öğretmen katılımcıyı görür, yetkisiz erişim, başlat, doğru cevap öğrenciye gitmez, geç katılım engeli, sonraki soru, bitir |
| Hafta 8 · Düzeltme + regresyon | Açık oturumlu quiz yayından kaldırılamaz, kilitli sorular, silinemeyen quiz, listeler, sürüm numarası |

**Gerçek veritabanına dokunmaz:** Test kendine geçici, boş bir veritabanı açıyor (`ConnectionStrings__Default` ortam değişkeniyle), sonunda siliyor. Yani demo verilerim bozulmuyor.

## Sürümleme (Semantic Versioning)

`MAJOR.MINOR.PATCH` → `0.8.0`

| Parça | Ne zaman artar | Bizde |
|---|---|---|
| MAJOR | Eski kullanımı bozan büyük değişiklik | `0` → proje henüz geliştirmede. Final sürümü `1.0.0` olacak |
| MINOR | Yeni özellik, eskiler bozulmadan | Her hafta yeni modül → `0.8` = 8. hafta |
| PATCH | Sadece hata düzeltmesi | Vizeden sonra küçük bir düzeltme olursa `0.8.1` |

**Git tag:** Belirli bir commit'e kalıcı isim vermek. `git tag -a v0.8.0 -m "Vize ara sürümü"` → `git push origin v0.8.0`. Hoca "vizede gösterdiğin kod hangisi?" derse GitHub'da `v0.8.0` etiketine bakılır; sonra kod değişse bile o sürüm aynen durur.

## Gösterilecek ilerleme

**Kullanıcı + veritabanı + çekirdek modüller uçtan uca çalışır gösterilir** → [demo senaryosu](../demo-senaryosu.md)

## Kontrol kriterleri

- [x] Ara sürüm çalışıyor (`v0.8.0`, kendi bilgisayarımda entegrasyon testi 38/38)
- [x] Modüller entegre (Kullanıcı → Quiz → Oturum, tek akışta)
- [x] Öğrenci tüm kodu açıklayabiliyor → aşağıdaki sorular + her haftanın notları

---

## Sözlü sınav soruları — cevaplar

### 1) Şu ana kadarki mimarini özetle.

> "Projem **C# ve ASP.NET Core MVC** ile yazılmış bir web uygulaması, veritabanı **SQLite**, aradaki bağlantıyı **Entity Framework Core** yapıyor. **Katmanlı mimari** kullandım, 4 proje var:
> - **Entities:** Tabloların C# sınıfları (User, Quiz, Question, QuizSession, Participant...)
> - **Data:** Veritabanı bağlantısı (`DbContext`), tablo ayarları, migration'lar ve repository'ler. Veritabanına sadece bu katman erişiyor.
> - **Business:** İş kuralları ve doğrulamalar. `AuthService`, `QuizService`, `SessionService`. 'Bu quiz bu öğretmenin mi', 'en az 2 şık var mı' gibi kontroller burada.
> - **Web:** Controller'lar ve sayfalar. Kullanıcının isteğini alıp servise iletiyor.
>
> Bağımlılık tek yönlü: Web → Business → Data → Entities. Katmanlar arası veriyi DTO ile taşıyorum. Giriş çerezle yapılıyor, roller öğretmen ve öğrenci."

📂 Göster: `docs/mimari.md` (şema) ve Visual Studio'da Solution Explorer'daki 4 proje

### 2) Hangi modül hangi veriyi nereden alıyor?

> | Modül | Controller → Servis → Repository → Tablo |
> |---|---|
> | Kullanıcı | `AccountController` → `AuthService` → `UserRepository` → `Users` |
> | Quiz | `QuizController` → `QuizService` → `QuizRepository`, `QuestionRepository` → `Quizzes`, `Questions`, `AnswerOptions` |
> | Oturum | `SessionController` / `StudentController` → `SessionService` → `SessionRepository` → `QuizSessions`, `Participants` |
>
> "Modüller birbirinden de veri alıyor: Quiz ve Oturum modülleri **giriş yapan kullanıcının Id'sini ve rolünü çerezden** alıyor. Oturum modülü **quizi ve sorularını** `QuizRepository` ile Quiz modülünden okuyor. Quiz modülü de **oturum tablosuna** bakıp sorular kilitli mi, quiz silinebilir mi karar veriyor. Öğrenci ekranı da 3 saniyede bir `PlayState` adresinden JSON olarak durumu alıyor."

📂 Göster: `docs/mimari.md` → "Modüller ve veri akışı" tablosu

### 3) En çok zorlandığın entegrasyon neydi, nasıl çözdün?

> "İki tane anlatacağım:
>
> **1. Quiz ile Oturum modülünün birbirini etkilemesi.** Hafta 8'de modülleri birlikte gözden geçirirken (PDF'te dendiği gibi yapay zekâyı entegrasyon hatalarını ayıklamak için kullandım) şunu fark ettik: oturum açıkken öğretmen quizi yayından kaldırabiliyordu, öğrenciler de bekleme salonunda taslak bir quizle kalıyordu. İki modül ayrı ayrı doğru çalışıyordu ama **birlikte** hata veriyordu, entegrasyon hatası buydu. Çözüm: `QuizRepository`'ye `HasOpenSessionAsync` metodunu ekledim, `QuizService` yayından kaldırmadan önce bunu soruyor. Bu tür hataları yakalamak için de **entegrasyon testi** yazdım.
>
> **2. Projenin kendi bilgisayarımda çalışmaması.** Kod hatasız derleniyordu ama site açılmıyordu. Test betiğine hata kaydı ekledim ve sebebini buldum: Windows'un **Akıllı Uygulama Denetimi**, Visual Studio'nun yeni derlediği imzasız `QuizArena.Data.dll` dosyasını engelliyordu. Ayarı kapatınca çalıştı. Buradan şunu öğrendim: kodun doğru olması yetmiyor, çalıştığı ortam da entegrasyonun parçası."

### 4) Yapay zekâdan aldığın bir kod parçasını satır satır açıkla.

**Katılım kodu üreten metot** — `SessionService.cs` → `CreateUniqueJoinCodeAsync`

```csharp
private async Task<string> CreateUniqueJoinCodeAsync()
{
    while (true)
    {
        var chars = new char[SessionRules.JoinCodeLength];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = SessionRules.JoinCodeChars[Random.Shared.Next(SessionRules.JoinCodeChars.Length)];
        }
        string code = new string(chars);

        if (!await _sessionRepository.JoinCodeExistsAsync(code))
            return code;
    }
}
```

| Satır | Açıklama |
|---|---|
| `private async Task<string> ...` | Sadece bu sınıfın içinden çağrılan (`private`), veritabanını beklediği için asenkron (`async`) bir metot; sonunda bir metin (`string`) döndürüyor |
| `while (true)` | Benzersiz bir kod bulana kadar tekrar et. Çıkış noktası aşağıdaki `return` |
| `new char[SessionRules.JoinCodeLength]` | 6 karakterlik boş bir dizi. 6 sayısını koda gömmedim, `SessionRules` sınıfındaki sabitten alıyorum |
| `for (int i = 0; ...)` | Dizinin 6 kutusunu tek tek dolduruyorum |
| `Random.Shared.Next(...)` | 0 ile izin verilen karakter sayısı arasında rastgele bir sayı |
| `SessionRules.JoinCodeChars[...]` | O sıradaki karakteri alıyorum. Bu listede **0/O ve 1/I yok**, öğrenci tahtadan yanlış okumasın diye ben çıkardım |
| `new string(chars)` | Karakter dizisini metne çeviriyorum, ör. `K7P2QX` |
| `JoinCodeExistsAsync(code)` | Veritabanına "bu kod başka bir oturumda var mı?" diye soruyorum (`await` = cevabı bekle) |
| `if (!...) return code;` | Yoksa kodu döndürüp çıkıyorum; varsa döngü başa dönüp yeni kod üretiyor |

> "Ayrıca veritabanında `JoinCode` sütunu **UNIQUE**; kod kontrolü bir şekilde kaçsa bile veritabanı aynı kodu kabul etmez. Bu metodu yapay zekâ yazdı; ben her satırını anladıktan sonra kullandım ve entegrasyon testinde kodun üretildiğini, küçük harfle yazılınca da çalıştığını kontrol ettim. AI günlüğümde de kayıtlı."

### 5) Bir sonraki aşamanın planı nedir?

> | Hafta | Plan |
> |---|---|
> | **9 · Gerçek zamanlı** | Şu anki 3 saniyelik kontrolü (polling) **SignalR / WebSocket** ile değiştireceğim. Soru herkese aynı anda düşecek, öğrenci **cevap verebilecek**. Bağlantı koparsa otomatik yeniden bağlanma olacak |
> | **10 · AI soru üretimi** | Öğretmen bir konu yazınca yapay zekâ soru önerecek; öneri yine `QuizValidator`'dan geçecek, öğretmen onaylarsa quize eklenecek. API anahtarı ortam değişkeninde duracak |
> | **11 · Puan / lider tablosu** | Doğru ve hızlı cevaba göre puan, lider tablosu, güç kartları |
> | **12 · Analiz** | Oturum sonu soru bazında doğru oranı, en zor soru, grafikler |
> | **13–15** | Birim testleri, dokümantasyon, dağıtım ve `v1.0.0` final sürümü |
>
> "Veritabanında `ParticipantAnswers` tablosu ve puan alanları Hafta 4'ten beri hazır, o yüzden Hafta 9 ve 11'de büyük bir yapı değişikliği gerekmeyecek."

---

## Teorik kavramlar

- **Entegrasyon:** Ayrı yazılmış modüllerin birlikte, tek bir akış olarak çalışması. Her modül tek başına doğru olsa bile birleşince hata çıkabilir (bizdeki 1. hata gibi).
- **Sürümleme:** Yazılımın belirli hâllerine numara vermek (`0.8.0`) ve Git tag ile etiketlemek. Hangi sürümün nerede çalıştığı belli olur, geri dönülebilir.
- **Regresyon:** Yeni bir değişikliğin **eskiden çalışan bir şeyi bozması**. Entegrasyon testindeki "Regresyon" kontrolleri, Hafta 8 değişikliklerinin Hafta 4–7'yi bozmadığını gösteriyor. `hafta7-test.bat` de hâlâ çalışıyor.

## Sık yapılan hatalar ve bizdeki durum

| Hata | Bizde |
|---|---|
| Entegrasyonu son ana bırakmak | Her hafta önceki modüllere bağlanarak ilerledim; Hafta 8'de otomatik testle doğruladım |
| Ara sürümü etiketlememek | `v0.8.0` etiketi + GitHub Release var |
| Açıklanamayan AI kodu taşımak | Her hafta notlarında kodun açıklaması ve AI günlüğü var; "Açıklar mı?" sütunu E |
