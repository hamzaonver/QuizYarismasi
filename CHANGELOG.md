# Sürüm Geçmişi (CHANGELOG)

Sürüm numaraları **semantic versioning** (anlamsal sürümleme) kuralına göre verilir: `MAJOR.MINOR.PATCH`

- **MAJOR** (büyük): Eski kullanımı bozan büyük değişiklik. Proje tamamlanınca `1.0.0` olacak.
- **MINOR** (küçük): Yeni özellik eklendi, eskiler bozulmadı. Bu projede her hafta bir MINOR artış gibi düşünüyorum.
- **PATCH** (yama): Sadece hata düzeltmesi.

`0.x.y` olması, projenin henüz geliştirme aşamasında olduğu anlamına gelir.
Her sürüm GitHub'da **Git tag** ile etiketlenir (ör. `v0.8.0`).

---

## [0.8.0] — 12.11.2026 · Vize ara sürümü (Hafta 8)

**Uçtan uca çalışan modüller:** Kullanıcı yönetimi (Hafta 5) + Veritabanı (Hafta 4) + Quiz oluşturma (Hafta 6) + Canlı oturum ve katılım (Hafta 7)

### Eklendi
- Uçtan uca **entegrasyon testi**: `scripts/entegrasyon-testi.bat`. Geçici bir veritabanında kayıt, giriş, quiz oluşturma, soru ekleme, yayınlama, oturum açma, katılım, başlatma, sonraki soru ve bitirme akışını otomatik dener; gerçek veritabanına dokunmaz.
- Sayfanın altında sürüm numarası görünüyor (`v0.8.0`).
- Vize demo senaryosu: `docs/demo-senaryosu.md`

### Düzeltildi (bilinen hatalar)
- Açık (bekleyen ya da devam eden) oturumu olan bir quiz **yayından kaldırılabiliyordu**; öğrenciler bekleme salonunda taslak bir quizle kalıyordu. Artık önce oturumun bitirilmesi isteniyor.
- İki öğrenci **aynı anda aynı takma adla** katılırsa ikincisi hata sayfası görüyordu. Artık "Bu takma ad az önce alındı" mesajı çıkıyor.
- Ana sayfada "Canlı oturum" özelliği hâlâ "Yakında" yazıyordu; Hafta 7'de tamamlandığı için düzeltildi.

## [0.7.0] — 05.11.2026 · Hafta 7
- Canlı oturum ve katılım: katılım kodu, bekleme salonu, Başlat / Sonraki soru / Bitir / İptal, 3 saniyelik durum kontrolü.
- Hafta 7 testi: `scripts/hafta7-test.bat`

## [0.6.0] — 15.10.2026 · Hafta 6
- Quiz oluşturma: quiz, soru, 2–6 şık, tek doğru cevap, süre ve puan; yayınla/taslak, kopyala, soru sırala.

## [0.5.0] — 15.10.2026 · Hafta 5
- Kayıt ve giriş, BCrypt ile parola hash, öğretmen/öğrenci rolleri, hesap kilitleme, CSRF koruması.

## [0.4.0] — 15.10.2026 · Hafta 4
- Veritabanı: EF Core migration, ilişkiler, indeksler, sentetik veri, Repository katmanı.

## [0.3.0] — 15.10.2026 · Hafta 3
- Proje iskeleti (4 katman), ER diyagramı, mimari şema, GitHub deposu.
