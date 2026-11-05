# Haftalık İlerleme Raporu (Hafta 3–7)

> Bilgisayar Uygulamaları I · Formlar ve Şablonlar, **Şablon 9**. Yazdırılabilir sürüm: `Haftalik-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | 
|---|---|---|
|  |  | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) |

## Hafta 3 — Kurulum, Mimari ve Veri Modeli

**Tarih:** 15.10.2026

**Bu hafta tamamlanan işler**

- Teknoloji yığınını seçtim: C# / .NET 8, ASP.NET Core MVC, Entity Framework Core, SQLite.
- Katmanlı proje iskeletini kurdum: QuizArena.Entities, Data, Business, Web (4 proje, tek çözüm).
- GitHub deposunu açtım; README ve .gitignore ekledim (bin/, obj/, *.db, .env depoya gitmiyor).
- ER diyagramını hazırladım: kullanıcı (öğretmen/öğrenci), quiz, soru, şık, oturum, katılımcı, cevap.
- Mimari şemayı çizdim (docs/mimari.png).

**Karşılaşılan sorunlar**

- Visual Studio'da projeyi çalıştırınca "başlangıç projesi" hatası aldım (sınıf kitaplığı çalıştırılmaya çalışılıyordu).
- İlk üretilen kod bir öğrenci projesi için fazla karmaşıktı, sunumda anlatmam zor olacaktı.

**Uygulanan çözümler**

- QuizArena.Web projesini başlangıç projesi (Set as Startup Project) yaptım.
- Kodu sadeleştirdim: servis arayüzlerini kaldırdım, DTO'ları basit sınıflara çevirdim, Türkçe açıklama ekledim.

**Yapay zekâ kullanımı özeti (günlüğe atıf)**

Mimari ve klasör yapısı önerisi, ER varlık/ilişki önerisi ve hata ayıklama için kullandım (AI günlüğü 1, 2, 6, 7).

**Sonraki hafta planı**

Veritabanı tablolarını migration ile oluşturmak, ilişkileri/indeksleri kurmak, sentetik veri ve CRUD.

**Ekler**

- Depo: https://github.com/hamzaonver/QuizYarismasi
- ER diyagramı: docs/er-diyagrami.png
- Mimari şema: docs/mimari.png

---

## Hafta 4 — Veritabanı Tasarımı ve Veri Erişim Katmanı

**Tarih:** 15.10.2026

**Bu hafta tamamlanan işler**

- Tabloları EF Core migration ile oluşturdum (InitialCreate). SQL karşılığı: docs/database/migration.sql.
- İlişkileri, kısıtları (UNIQUE, CHECK, FK) ve indeksleri QuizArenaDbContext içinde tanımladım.
- Sentetik veriyle doldurdum (DbInitializer): demo öğretmen/öğrenciler, 3 quiz. Gerçek kişi verisi yok (KVKK).
- Temel CRUD için IRepository<T> / Repository<T> ve özel depolar (User, Quiz, Question) yazıldı.

**Karşılaşılan sorunlar**

- Migration'ı ilk kez çalıştırırken komutların nerede ve hangi sırayla yazılacağını bilmiyordum.
- Oluşan veritabanının içini görmek için bir araca ihtiyacım vardı.

**Uygulanan çözümler**

- Migration'ı kendi bilgisayarımda oluşturup Update-Database ile veritabanını kurdum.
- DB Browser for SQLite kurup tabloları, ilişkileri ve sentetik veriyi kontrol ettim.

**Yapay zekâ kullanımı özeti (günlüğe atıf)**

Şema, indeks önerisi ve migration örnekleri için kullandım (AI günlüğü 3).

**Sonraki hafta planı**

Kayıt/giriş, parola hash'leme ve rol tabanlı erişim (öğretmen/öğrenci).

**Ekler**

- Commit: https://github.com/hamzaonver/QuizYarismasi/commit/2341a3a
- Migration: src/QuizArena.Data/Migrations/
- SQL: docs/database/schema.sql, crud-ornekleri.sql

---

## Hafta 5 — Kullanıcı Yönetimi ve Kimlik Doğrulama

**Tarih:** 15.10.2026

**Bu hafta tamamlanan işler**

- Kayıt ve giriş ekranları (AccountController + AuthService).
- Parolalar BCrypt ile hash'leniyor, düz metin saklanmıyor.
- Parola kuralı: en az 8 karakter, büyük harf, küçük harf ve rakam.
- Çerez tabanlı oturum (HttpOnly, 2 saat) ve roller: Teacher / Student.
- 5 hatalı girişte hesap 5 dakika kilitleniyor; tüm formlarda CSRF koruması var.
- Kısa güvenlik notu yazıldı (docs/guvenlik-notu.md).

**Karşılaşılan sorunlar**

- Herkesin kendini öğretmen olarak kaydedebilmesi güvenlik açığı olurdu.
- Öğrencinin, adresi elle yazarak öğretmen sayfalarına girmesi engellenmeliydi.

**Uygulanan çözümler**

- Öğretmen kaydına davet kodu şartı koydum.
- Controller'lara [Authorize(Roles = ...)] ekledim; sahiplik kontrolünü iş katmanında yaptım.

**Yapay zekâ kullanımı özeti (günlüğe atıf)**

Kimlik doğrulama yapısı, parola hash'leme ve güvenlik kontrolleri için kullandım (AI günlüğü 4).

**Sonraki hafta planı**

Quiz oluşturma modülü: quiz, soru, şık, doğru cevap, süre.

**Ekler**

- Güvenlik notu: docs/guvenlik-notu.md
- Demo hesaplar: ogretmen@quizarena.test, ogrenci1@quizarena.test

---

## Hafta 6 — Çekirdek Modül A: Quiz Oluşturma

**Tarih:** 15.10.2026

**Bu hafta tamamlanan işler**

- Quiz → Soru → Şık yapısı; her soruda 2–6 şık ve tek doğru cevap.
- Quiz için varsayılan süre, her soru için ayrı süre (5–300 sn) ve puan.
- Quiz oluştur, düzenle, sil, yayınla/taslağa al, kopyala.
- Soru ekle, düzenle, sil, yukarı/aşağı taşı.
- İş kuralları QuizValidator ve QuizRules içinde; sahiplik kontrolü QuizService'te.

**Karşılaşılan sorunlar**

- Doğru cevabın öğrenciye gösterilip gösterilmeyeceği belirsizdi.
- Oturumu açılmış bir quizin soruları değişirse sonuçlar bozulabilirdi.

**Uygulanan çözümler**

- Doğru cevabı sadece öğretmen ekranında gösterme kararı aldım (öğrenciye hiç gönderilmiyor).
- Oturumu olan quizin sorularını kilitledim; sadece kopyalayıp yeni quiz olarak düzenlenebiliyor.

**Yapay zekâ kullanımı özeti (günlüğe atıf)**

Modül kodu, doğrulama kuralları ve kavramları (DTO, entity) anlamak için kullandım (AI günlüğü 5, 9, 10).

**Sonraki hafta planı**

Canlı oturum: katılım kodu, öğrencinin oturuma katılması, oturum durumu.

**Ekler**

- Commit: https://github.com/hamzaonver/QuizYarismasi/commit/2341a3a
- Notlar: docs/haftalar/hafta-06.md

---

## Hafta 7 — Çekirdek Modül B: Canlı Oturum ve Katılım + Entegrasyon

**Tarih:** 05.11.2026

**Bu hafta tamamlanan işler**

- Öğretmen yayındaki quizden oturum açıyor; 6 karakterlik benzersiz katılım kodu üretiliyor.
- Öğrenci kod ve takma adla katılıyor, bekleme salonuna düşüyor.
- Oturum durumları: Bekleme salonu → Devam ediyor → Bitti.
- Öğretmen ekranı: katılımcı listesi, Başlat / Sonraki soru / Bitir / İptal.
- Ekranlar 3 saniyede bir durumu kontrol ediyor; öğrenci ekranı soruya kendiliğinden geçiyor.
- Hafta 5 (roller) ve Hafta 6 (yayın durumu, soru kilidi) ile entegre edildi; yeni migration gerekmedi.

**Karşılaşılan sorunlar**

- Öğrenci sayfa kaynağına bakarak doğru cevabı görebilirdi.
- Başlamış bir oturuma sonradan giren öğrenci soruları kaçırırdı.
- Aynı takma adla iki öğrenci girerse lider tablosunda karışıklık olurdu.

**Uygulanan çözümler**

- Öğrenciye giden veride (LiveQuestionDto) doğru cevap alanını hiç koymadım.
- Sadece bekleme salonundaki oturuma katılmaya izin verdim.
- Aynı oturumda takma adı benzersiz yaptım (büyük/küçük harf fark etmeden).

**Yapay zekâ kullanımı özeti (günlüğe atıf)**

Oturum servisinin ve ekranların kodu için kullandım; doğru cevabın öğrenciye gitmemesi kuralını ben istedim (AI günlüğü 11, 12).

**Sonraki hafta planı**

Hafta 8 (vize): modülleri uçtan uca birleştirmek, bilinen hataları gidermek, ara sürümü Git tag ile etiketlemek (v0.8.0), demo senaryosunu prova etmek.

**Ekler**

- Commit: https://github.com/hamzaonver/QuizYarismasi/commit/d029321
- Notlar ve demo adımları: docs/haftalar/hafta-07.md

---
