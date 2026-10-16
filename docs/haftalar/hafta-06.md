# Hafta 6 — Çekirdek Modül A: Quiz Oluşturma

**Amaç:** Quiz ve soru oluşturma modülünü geliştirmek.

## Yapılanlar

- [x] Quiz/soru modeli: `Quiz` → `Question` → `AnswerOption` (1‑N‑N)
- [x] Şık/doğru cevap: 2‑6 şık, doğru cevap şık üzerinde `IsCorrect` alanı
- [x] Süre: quiz için varsayılan süre + her soru için ayrı süre (5‑300 sn) ve puan (0‑5000)
- [x] Quiz işlemleri: oluştur, düzenle, sil, yayınla/taslağa al, kopyala
- [x] Soru işlemleri: ekle, düzenle, sil, yukarı/aşağı taşı (sıra otomatik yeniden numaralanır)
- [x] İş kuralları ve doğrulama → `Business/Validation/QuizValidator.cs`, `QuizRules.cs`
- [x] Sahiplik kontrolü → `QuizService.CheckOwner`

## İş kuralları (neden konuldu?)

| Kural | Neden |
|---|---|
| Başlık 3‑150 karakter | Boş/anlamsız başlık listede ayırt edilemez |
| En az 2, en fazla 6 şık | Tek şıklı soru anlamsız; 6'dan fazlası canlı ekranda sığmaz |
| Tam olarak 1 doğru şık | Puanlama (Hafta 11) tek doğru cevaba göre çalışacak |
| Aynı şık iki kez yazılamaz (büyük/küçük harf duyarsız) | İki "doğru görünen" şık öğrenciyi yanıltır |
| Boş şık "doğru" işaretlenemez | Formda boş bırakılan kutular yok sayılıyor |
| Süre 5‑300 sn, puan 0‑5000 | Veritabanındaki CHECK kısıtlarıyla aynı; arayüz atlatılsa da veri bozulmaz |
| Soru içermeyen quiz yayınlanamaz | Hafta 7'de boş quizle oturum başlatılmasın |
| Son soru silinince quiz taslağa döner | Aynı neden |
| Bir quizde en fazla 50 soru | Canlı oturum süresini makul tutmak |
| **Oturumda kullanılmış quizin soruları kilitlenir** | Eski cevaplar sorulara/şıklara bağlı; soru değişirse geçmiş sonuç raporu yanlış olur. Değişiklik için "Kopyala" kullanılır |
| Oturumu olan quiz silinemez | Sonuç raporları kaybolmasın (DB'de de Restrict) |
| Öğretmen yalnızca kendi quizini görür/değiştirir | Erişim kontrolü (OWASP: Broken Access Control) |

## Teslim edilecekler

| Çıktı | Nerede |
|---|---|
| Çalışan Quiz Oluşturma | `/Quiz` (öğretmen girişiyle) |
| İlgili kod | `Web/Controllers/QuizController.cs`, `Business/Services/QuizService.cs`, `Business/Validation/`, `Data/Repositories/QuizRepository.cs`, `QuestionRepository.cs`, `Web/Views/Quiz/` |
| Kısa açıklama | Bu dosya |

## Gösterilecek ilerleme (demo)

1. `ogretmen@quizarena.test` / `Ogretmen123!` ile giriş → Quizlerim.
2. "+ Yeni quiz" → başlık "ab" yaz → doğrulama hatası; düzgün başlıkla oluştur.
3. "Yayınla" → "en az bir soru olmalı" hatası.
4. Soru ekle: tek şık + doğru seçmeden kaydet → iki hata birden. Aynı şıkkı iki kez yaz → hata. Düzgün soru ekle.
5. İkinci soru ekle (5 şıklı), sırasını yukarı taşı, düzenle, sil.
6. Yayınla → "Yayında" rozeti. Kopyala → "(Kopya)" taslak quiz.
7. "Bilgisayar Donanımı Temelleri" (örnek oturumu olan quiz) → "Sorular kilitli" uyarısı, düzenleme butonları yok.
8. İkinci öğretmen hesabıyla (`ogretmen2@…`) giriş yapıp ilk öğretmenin quiz adresini (`/Quiz/Details/1`) aç → Erişim engellendi.

## Kontrol kriterleri

- [x] Quiz Oluşturma çalışıyor
- [x] Doğrulama/iş kuralları uygulanıyor → hem arayüzde (DataAnnotations) hem sunucuda (QuizValidator)
- [x] Önceki modüllerle tutarlı → Hafta 5'in rol sistemi + Hafta 4'ün repository'leri kullanılıyor

## Senin kendi geliştirmen gereken kısımlar

Hoca "çekirdek mantığı ve iş kurallarını kendin yaz" diyor. Önerilen kendi eklemelerin (birini seç):
- **Yeni bir iş kuralı:** ör. "soru metni en az 5 karakter olmalı" ya da "aynı quizde aynı soru metni iki kez olamaz" → `QuizValidator.ValidateQuestion` içine ekle, formda dene.
- **Doğru/Yanlış soru tipi:** "Doğru/Yanlış" butonu tek tıkla iki şıkkı doldursun.
- **Quiz arama/filtre:** Quizlerim sayfasına "Yayında / Taslak" filtresi.

## Sözlü sınav soruları — cevap taslakları

**1) Quiz'i nasıl modelledin?**
Üç seviyeli bir yapı: `Quiz` (başlık, açıklama, varsayılan süre, yayın durumu, sahibi öğretmen) → `Question` (metin, sıra, süre, puan) → `AnswerOption` (metin, sıra, doğru mu). Quiz‑Soru ve Soru‑Şık 1‑N ilişki, Cascade silme. Sorulara `OrderNo` verdim çünkü canlı oturumda sorular bu sırayla gösterilecek.

**2) Doğru cevabı nasıl sakladın?**
Şık tablosunda `IsCorrect` (bool) sütunu olarak. Alternatif olarak soruya `CorrectOptionId` koyabilirdim ama o zaman soru ile şık arasında döngüsel FK oluşurdu ve önce soruyu, sonra şıkları, sonra tekrar soruyu kaydetmek gerekirdi. `IsCorrect` ile tek seferde kaydediliyor; "tam olarak bir doğru şık" kuralını iş katmanı garanti ediyor. Doğru cevap öğrenci ekranına hiç gönderilmeyecek (Hafta 7‑9'da bunu dikkatle yapacağım).

**3) Bu modülün verisini nasıl modelledin?**
Arayüzden gelen veriyi önce `QuestionFormViewModel` alıyor (6 şık kutusu + doğru şık radyo butonu), sonra `QuestionInputDto`'ya çevriliyor; servis bu DTO'yu doğrulayıp `Question` ve `AnswerOption` varlıklarına dönüştürüyor. Arayüze ise `QuizDetailDto` / `QuestionDto` gidiyor. Varlıkları doğrudan view'a vermiyorum; böylece istenmeyen alanların formdan değiştirilmesini (over‑posting) engelliyorum, ör. `TeacherId` formdan alınmıyor, oturumdaki kullanıcıdan geliyor.

**4) Bu modülde hangi iş kuralını neden koydun?**
En önemlisi "oturumda kullanılmış quizin soruları kilitlenir". Öğrenci cevapları soru ve şık id'lerine bağlı; soru sonradan değişirse geçmiş oturumun raporu yanlış olur (öğrenci aslında başka bir soruya cevap vermiş gibi görünür). Bu yüzden kilitliyorum ve değişiklik için "Kopyala" sunuyorum. Diğerleri yukarıdaki tabloda.

**5) Bu modülü diğer modüllerle nasıl entegre ettin?**
Hafta 5'teki rol sistemiyle: `QuizController` sadece öğretmenlere açık, öğretmen id'si oturum çerezinden (`TeacherId` özelliği, `ClaimTypes.NameIdentifier`) geliyor. Hafta 4'teki veri katmanıyla: `QuizService` sadece repository arayüzlerini kullanıyor, DbContext'i doğrudan görmüyor. Hafta 7 için: `IsPublished` alanı sadece yayındaki quizlerle oturum başlatılabilmesi için, `IsUsedInSessionAsync` da oturum modülünün oluşturacağı kayıtlara bakıyor.

## Teorik kavramlar (kısa)

- **Veri modeli**, **iş kuralı / doğrulama** (istemci tarafı = kolaylık, sunucu tarafı = güvenlik)
- **Modülerlik:** her modül kendi servisinde; ortak altyapı (Result, Repository) paylaşılıyor
- **Over‑posting**, **IDOR** (başkasının kaydına id değiştirerek erişme) ve sahiplik kontrolü
