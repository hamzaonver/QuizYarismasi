# Hafta 3 — Kurulum, Mimari ve Veri Modeli

**Amaç:** Geliştirme ortamını kurmak, proje mimarisini ve veri modelini tasarlamak, GitHub deposunu oluşturmak.

## Yapılanlar

- [x] Teknoloji yığını seçildi: C# / .NET 8, ASP.NET Core MVC, EF Core, SQLite → gerekçe: [`../mimari.md`](../mimari.md)
- [x] Boş proje iskeleti ve katmanlı klasör yapısı: `QuizArena.sln` + 4 proje (`Entities`, `Data`, `Business`, `Web`)
- [x] `.gitignore` (Visual Studio/.NET şablonu + `*.db`, `.env`, `appsettings.*.local.json`) ve `README.md`
- [x] ER diyagramı: öğretmen (Users), quiz, soru, şık, oturum, katılımcı, cevap → [`../er-diyagrami.md`](../er-diyagrami.md), [`../er-diyagrami.png`](../er-diyagrami.png)
- [x] Mimari şema → [`../mimari.png`](../mimari.png)

## Teslim edilecekler

| Çıktı | Nerede |
|---|---|
| GitHub deposu (README + .gitignore) | Depo kökü |
| ER diyagramı | `docs/er-diyagrami.png` ve `.md` |
| Mimari şema | `docs/mimari.png` ve `.md` |

## Gösterilecek ilerleme (demo)

1. GitHub'da depoyu aç: klasör yapısını göster (`src/` altında 4 katman).
2. Visual Studio'da `QuizArena.sln` → Solution Explorer'da proje referanslarını göster (Web → Business → Data → Entities).
3. `dotnet run --project src/QuizArena.Web` → boş iskelet çalışıyor.
4. ER diyagramını açıp varlıkları ve ilişkileri anlat.

## Kontrol kriterleri

- [x] Depo erişilebilir ve düzenli → gereksiz dosya yok (`bin/`, `obj/`, `*.db` .gitignore'da)
- [x] ER diyagramı varlık/ilişkileri doğru → 7 varlık, 1‑N ve N‑N ilişkiler, PK/FK işaretli
- [x] Mimari katmanlar tanımlı → `docs/mimari.md` sorumluluk tablosu

## Senin kendi geliştirmen gereken kısımlar

Hocanın beklentisi "varlıkları kendin tanımla, klasör yapını gerekçelendir". Sözlüde rahat olmak için:
- ER diyagramındaki her ilişkiyi kendi cümlenle bir kâğıda yaz ("Bir öğretmenin birden çok quizi olur, çünkü…").
- Bir varlığa kendi fikrin olan bir alan ekle (ör. `Quiz.Category` ya da `User.AvatarColor`) → Entity + Configuration + ER diyagramını güncelle. Bu, modeli sahiplendiğini gösterir.
- `docs/mimari.md` içindeki "Değerlendirilen alternatifler" tablosunu kendi düşüncenle genişlet.

## Sözlü sınav soruları — cevap taslakları

> Bunları ezberleme; okuyup kendi cümlelerinle anlat.

**1) Katmanlı mimariyi neden tercih ettin?**
Her katmanın tek bir sorumluluğu var: Web sadece HTTP ve sayfalarla, Business iş kurallarıyla, Data veritabanıyla ilgileniyor. Bir kuralı (ör. "en az 2 şık") değiştirmem gerektiğinde nereye bakacağımı biliyorum. Arayüzü değiştirsem bile iş kuralları aynı kalıyor ve servisleri arayüzden bağımsız test edebiliyorum. Bağımlılık tek yönlü: üst katman alttakini kullanır, tersi olmaz.

**2) İlişki türlerini (1‑1, 1‑N, N‑N) örnekle açıkla.**
- 1‑N: Bir quizin birçok sorusu var, her soru tek bir quize ait → `Questions.QuizId` FK.
- N‑N: Bir öğrenci birçok oturuma katılır, bir oturumda birçok öğrenci olur → ara tablo `Participants` (SessionId + UserId).
- 1‑1: Bu projede zorunlu bir 1‑1 yok. Örnek olarak kullanıcı ve kullanıcı profil ayarları ayrı tabloda tutulsaydı 1‑1 olurdu (profil tablosunda UserId hem FK hem UNIQUE).

**3) Hangi varlıklar birincil anahtar taşıyor ve neden?**
Hepsi. Her tabloda otomatik artan `Id` var (yapay anahtar). Kısa, değişmez ve FK olarak kullanması kolay. E‑posta gibi doğal bir alanı PK yapmadım çünkü değişebilir; onun yerine UNIQUE indeks koydum.

**4) Klasör yapını hangi mantıkla kurdun?**
Önce katmana göre proje ayırdım, sonra her projenin içinde görev türüne göre klasörledim (Services, Validation, Repositories, Helpers). Yeni bir modül eklerken her katmanda ilgili klasöre birer dosya ekliyorum; neyin nerede olduğu tahmin edilebilir.

**5) .gitignore neden gereklidir, neler eklenmeli?**
Depoya sadece kaynak kodun girmesini sağlar. Derleme çıktıları (`bin/`, `obj/`), IDE dosyaları (`.vs/`), yerel veritabanı (`*.db`) ve gizli bilgiler (`.env`, yerel appsettings) eklenmemeli. Gizli anahtar bir kez depoya girerse geçmişte kalır; silmek yetmez, anahtarı değiştirmek gerekir.

## Teorik kavramlar (kısa)

- **Katmanlı mimari:** sunum / iş / veri ayrımı
- **ER modeli:** varlık, nitelik, ilişki; PK (birincil anahtar), FK (yabancı anahtar)
- **Normalizasyon:** 1NF atomik değer, 2NF kısmi bağımlılık yok, 3NF geçişli bağımlılık yok → bkz. `er-diyagrami.md`
- **Sürüm kontrolü:** commit, branch, push; anlamlı commit mesajları
