# Hafta 7 — Çekirdek Modül B: Canlı Oturum ve Katılım + Entegrasyon

**Amaç:** Canlı oturum ve katılımcı yönetimi modülünü geliştirmek.

## Yapılanlar

- [x] **Oturum modeli:** Öğretmen yayındaki bir quizden oturum açar, sistem 6 haneli **katılım kodu** üretir (ör. `K7P2QX`)
- [x] **Katılımcı katılımı:** Öğrenci panelden kodu ve takma adını girerek oturuma katılır
- [x] **Durum:** Oturum üç durumdan geçer → **Bekleme salonu → Devam ediyor → Bitti**
- [x] Öğretmen ekranı: katılım kodu, katılımcı listesi (3 saniyede bir kendiliğinden güncellenir), Başlat / Sonraki soru / Bitir / İptal
- [x] Öğrenci ekranı: bekleme salonu → mevcut soru → oturum bitti (kendiliğinden geçer)
- [x] "Oturumlarım" (öğretmen) ve "Katıldığım oturumlar" (öğrenci) listeleri

> Veritabanı tabloları (`QuizSessions`, `Participants`) Hafta 4'te hazırlanmıştı. Bu hafta **yeni migration gerekmedi**.

## Dosyalar

| Dosya | Görevi |
|---|---|
| `QuizArena.Data/Repositories/ISessionRepository.cs`, `SessionRepository.cs` | Oturum sorguları (koda göre bul, öğretmenin/öğrencinin oturumları) |
| `QuizArena.Business/Services/SessionService.cs` | **Asıl mantık:** oturum aç, katıl, başlat, sonraki soru, bitir, iptal |
| `QuizArena.Business/Validation/SessionRules.cs` | Kural sabitleri (kod uzunluğu, takma ad, en fazla katılımcı) |
| `QuizArena.Business/Dtos/SessionDtos.cs` | Ekranlara giden veriler (**doğru cevap içermez**) |
| `QuizArena.Web/Controllers/SessionController.cs` | Öğretmen ekranları (sadece öğretmen) |
| `QuizArena.Web/Controllers/StudentController.cs` | Öğrenci paneli, katılım, oturum ekranı (sadece öğrenci) |
| `QuizArena.Web/Models/SessionViewModels.cs` | Katılım formu |
| `QuizArena.Web/Views/Session/` | `Index` (Oturumlarım), `Host` (öğretmen oturum ekranı) |
| `QuizArena.Web/Views/Student/` | `Index` (panel + katılım), `Play` (öğrenci oturum ekranı) |
| `QuizArena.Web/wwwroot/js/site.js` | `startStatePolling`: 3 sn'de bir durum kontrolü |

## İş kuralları (neden konuldu?)

| Kural | Neden | Kod |
|---|---|---|
| Sadece **yayındaki ve sorusu olan** quizle oturum açılır | Taslak veya boş quiz sınıfa açılmasın | `SessionService.CreateAsync` |
| Aynı quiz için bitmemiş oturum varsa **yenisi açılmaz**, var olana gidilir | Aynı quizden iki oturum karışmasın | `CreateAsync` |
| Katılım kodu **6 karakter, benzersiz**; 0/O, 1/I harfleri yok | Öğrenci yanlış okumasın, iki oturum aynı kodu almasın | `CreateUniqueJoinCodeAsync` |
| Kod **büyük/küçük harf ve boşluk fark etmez** | `k7p2qx ` yazan da girebilsin | `NormalizeJoinCode` |
| Sadece **bekleme salonundaki** oturuma katılınır | Başlamış quize sonradan girip soru kaçırmasın | `JoinAsync` |
| Aynı oturumda **aynı takma ad olamaz** | Lider tablosunda kim kim karışmasın | `JoinAsync` (+ veritabanında UNIQUE) |
| Takma ad 2–20 karakter, en fazla **50 katılımcı** | Ekrana sığsın, oturum makul kalsın | `SessionRules` |
| Aynı öğrenci tekrar katılırsa **yeni kayıt açılmaz**, oturumuna döner | Sayfayı kapatıp açan öğrenci iki kez sayılmasın | `JoinAsync` |
| **En az 1 katılımcı** olmadan oturum başlatılamaz | Boş sınıfa quiz başlatılmasın | `StartAsync` |
| Sadece başlamamış oturum **iptal** edilir | Başlamış oturumun kaydı korunsun | `CancelAsync` |
| Oturumu sadece **açan öğretmen** yönetir; öğrenci sadece **katıldığı** oturumu görür | Erişim/sahiplik kontrolü | `CheckHost`, `GetStudentViewAsync` |
| Öğrenciye **doğru cevap gönderilmez** | Sayfa kaynağına bakıp kopya çekilmesin | `LiveQuestionDto` (IsCorrect yok) |

## Teslim edilecekler

| Çıktı | Nerede |
|---|---|
| Çalışan Canlı Oturum ve Katılım | Öğretmen: quiz sayfası → "Canlı oturum başlat"; Öğrenci: Panelim |
| İlgili kod | Yukarıdaki dosya tablosu |
| Kısa açıklama | Bu dosya |

## Gösterilecek ilerleme (demo)

> İki tarayıcı penceresi aç: biri normal, biri **gizli pencere** (Ctrl+Shift+N). Böylece öğretmen ve öğrenci aynı anda giriş yapabilir.

1. **Öğretmen** (`ogretmen@quizarena.test` / `Ogretmen123!`) → Quizlerim → "Bilgisayar Donanımı Temelleri" → **▶ Canlı oturum başlat** → büyük katılım kodu görünür.
2. Hemen **Oturumu başlat**'a bas → *"En az bir öğrencinin katılması gerekir"* (kural).
3. **Öğrenci** (gizli pencere, `ogrenci1@quizarena.test` / `Ogrenci123!`) → Panelim → yanlış kod yaz → *"Bu koda ait bir oturum bulunamadı"*.
4. Doğru kodu **küçük harfle** yaz, takma ad "Ali" → **bekleme salonu**.
5. **Öğretmen ekranına bak:** sayfayı yenilemeden "Ali" listede belirir.
6. (İsteğe bağlı) Üçüncü pencerede `ogrenci2` ile aynı takma adı dene → *"Bu takma ad kullanılıyor"*.
7. Öğretmen **Oturumu başlat** → 1. soru. **Öğrenci ekranı kendiliğinden** 1. soruya geçer.
8. Öğretmen **Sonraki soru** → öğrenci 2. soruya geçer → … → **Son soru · Oturumu bitir** → öğrenci "Oturum sona erdi" görür.
9. Öğretmen **Oturumlarım**, öğrenci **Katıldığım oturumlar** listesini göster.
10. (İsteğe bağlı) Taslak quizde ("C# Temelleri") "Canlı oturum başlat" butonu **yok**.

## Kontrol kriterleri

- [x] Canlı Oturum ve Katılım çalışıyor
- [x] Doğrulama/iş kuralları uygulanıyor (yukarıdaki tablo)
- [x] Önceki modüllerle tutarlı: Hafta 5 rolleri (`[Authorize]`), Hafta 6 yayın durumu ve soru kilidi, Hafta 4 tabloları

## Sözlü sınav soruları — cevaplar

**1) Oturuma katılımı nasıl kurdun?**
> "Öğretmen yayındaki bir quizden oturum açınca sistem 6 karakterlik benzersiz bir katılım kodu üretiyor. Öğrenci panelinde bu kodu ve takma adını giriyor. `SessionService.JoinAsync` önce kodu büyük harfe çevirip oturumu buluyor, sonra kuralları kontrol ediyor: oturum bekleme salonunda mı, takma ad alınmış mı, oturum dolu mu. Hepsi uygunsa öğrenci katılımcı olarak ekleniyor ve bekleme ekranına geçiyor."

**2) Katılımcıyı oturuma nasıl bağladın?**
> "`Participants` tablosu üzerinden. Bu tablo oturum ile kullanıcı arasındaki N‑N ilişkinin ara tablosu. Her katılımcı satırında `SessionId` (hangi oturum) ve `UserId` (hangi öğrenci) var, ayrıca takma ad ve puan tutuluyor. `UserId`'yi formdan değil giriş yapan öğrenciden alıyorum. Öğrenci aynı oturuma tekrar katılmaya çalışırsa yeni satır açmıyorum, var olan kaydına yönlendiriyorum."

**3) Bu modülün verisini nasıl modelledin?**
> "İki tablo var: `QuizSessions` ve `Participants`. Oturumda quiz, oturumu açan öğretmen, katılım kodu, durum (Bekliyor / Aktif / Bitti) ve şu an gösterilen sorunun sırası tutuluyor. Durum bir enum olarak tutuluyor. Katılım kodu ve oturum + takma ad ikilisi veritabanında benzersiz. Ekrana giden veride (`LiveQuestionDto`) doğru cevap bilgisi bilerek yok."

**4) Bu modülde hangi iş kuralını neden koydun?**
> "En önemlisi: **sadece bekleme salonundaki oturuma katılınabilir**; başlamış quize sonradan giren öğrenci soruları kaçırır ve adil olmaz. Ayrıca aynı oturumda aynı takma ad olamaz, çünkü lider tablosunda kimin kim olduğu karışır. En az bir katılımcı olmadan oturum başlatılamaz. Öğrenciye doğru cevabı göndermiyorum ki sayfa kaynağından kopya çekemesin."

**5) Bu modülü diğer modüllerle nasıl entegre ettin?**
> "**Hafta 6 ile:** Oturum sadece yayındaki ve sorusu olan quizden açılabiliyor; oturum açılınca o quizin soruları kilitleniyor. **Hafta 5 ile:** Oturum ekranlarına sadece öğretmen, katılım ve oyun ekranına sadece öğrenci girebiliyor; öğretmen ve öğrenci Id'si giriş çerezinden alınıyor. **Hafta 4 ile:** Tablolar 4. haftada hazırlanmıştı, yeni migration gerekmedi; veri erişimi Repository üzerinden. **Hafta 9 için:** Şu an ekranlar 3 saniyede bir durumu soruyor, 9. haftada bunu WebSocket ile anlık hale getireceğim."

## Teorik kavramlar

- **Veri modeli:** Oturum ve katılımcı tabloları, N‑N ilişki (ara tablo)
- **Durum (state):** Oturumun Bekliyor → Aktif → Bitti şeklinde ilerlemesi; her durumda izin verilen işlemler farklı
- **İş kuralı / doğrulama:** Yukarıdaki tablo
- **Modülerlik:** Oturum modülü kendi servisinde (`SessionService`); quiz modülüne sadece `IQuizRepository` üzerinden erişiyor
- **Polling:** Sayfanın belirli aralıklarla sunucuya "değişiklik var mı?" diye sorması (Hafta 9'da WebSocket'e geçilecek)

## Sık yapılan hatalar ve bizdeki durum

| Hata | Bizde |
|---|---|
| Doğrulamayı atlamak | Kod, takma ad, durum, kapasite kontrolleri `JoinAsync` içinde |
| Erişim/sahiplik kontrolünü unutmak | Öğretmen sadece kendi oturumunu, öğrenci sadece katıldığı oturumu görüyor |
| İlişkili kayıtları yönetmemek | Oturum iptal edilince katılımcılar da siliniyor (Cascade) |
