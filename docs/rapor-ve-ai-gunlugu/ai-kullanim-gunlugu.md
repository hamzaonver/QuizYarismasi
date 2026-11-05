# Yapay Zekâ Kullanım Günlüğü (AI Usage Log)

> Bilgisayar Uygulamaları I · Formlar ve Şablonlar, **Şablon 8**. Yazdırılabilir sürüm: `AI-Kullanim-Gunlugu.docx`

**İlke:** Yapay zekâdan aldığım her anlamlı yardımı bu günlüğe işliyorum. "Açıklar mı?" sütunu "E" olmayan hiçbir kod parçası teslim edilemez.

| No | Tarih | Araç | Amaç/Görev | İstem (özet) | Öğrencinin Değişikliği/Doğrulaması | Açıklar mı? (E/H) |
|---|---|---|---|---|---|---|
| 1 | 15.10.2026 | Claude | Araştırma — Hafta 3 | P60 projesi için uygun teknoloji yığını ve katmanlı klasör yapısı nedir? | Seçenekler arasından C# ASP.NET Core MVC + SQLite'ı kendim seçtim; 4 katmanın görevlerini inceledim. | E |
| 2 | 15.10.2026 | Claude | ER modelleme — Hafta 3 | Quiz yarışması için varlık ve ilişki önerisi. | Varlıkları PDF'teki listeyle (öğretmen, quiz, oturum, katılımcı, cevap) karşılaştırdım; 1-N ve N-N ilişkileri kontrol ettim. | E |
| 3 | 15.10.2026 | Claude | Veritabanı — Hafta 4 | EF Core ile tablo, ilişki, indeks, migration ve sentetik veri. | Migration'ı kendi bilgisayarımda çalıştırdım; DB Browser for SQLite ile tabloları ve veriyi kontrol ettim. | E |
| 4 | 15.10.2026 | Claude | Kimlik doğrulama — Hafta 5 | Kayıt/giriş, parola hash'leme ve rol tabanlı erişim nasıl yapılır? | Demo hesaplarla giriş yapıp öğretmen ve öğrenci panellerinin farklı olduğunu denedim. | E |
| 5 | 15.10.2026 | Claude | Kod önerisi — Hafta 6 | Quiz/soru/şık oluşturma modülü ve doğrulama kuralları. | Quiz oluşturma, soru ekleme ve yayınlama işlemlerini sitede denedim. | E |
| 6 | 15.10.2026 | Claude | Hata ayıklama | Visual Studio'da proje çalışmıyor (ekran görüntüsü ile). | Önerilen çözümü uyguladım: QuizArena.Web'i başlangıç projesi yaptım, proje çalıştı. | E |
| 7 | 15.10.2026 | Claude | Refaktör | Kodlar profesyonele yakın ama tam profesyonel olmayacak şekilde sadeleştirilsin. | Sade sürümü kabul ettim; anlatamayacağım kısımları (servis arayüzleri vb.) çıkarttırdım. | E |
| 8 | 16.10.2026 | Claude | Sürüm kontrolü | GitHub deposunu bağlama ve projeyi yükleme. | GitHub uygulamasını kendi hesabıma kurdum, yüklemenin depoda göründüğünü kontrol ettim. | E |
| 9 | Ekim 2026 | Claude | Kavram öğrenme | DTO nedir, entity nedir, birincil anahtar, katmanlı mimari; sözlü sorulara nasıl cevap veririm? | Cevapları kendi cümlelerimle kısalttım; sunumda kodu göstererek anlattım. | E |
| 10 | Ekim 2026 | Claude | Tasarım kararı — Hafta 6 | Doğru şık, öğrenci işaretledikten sonra öğrenciye gösterilecek mi, yoksa sadece öğretmene mi? | Doğru cevabın sadece öğretmen ekranında olmasına ben karar verdim; bu kuralı Hafta 7'de de istedim. | E |
| 11 | 05.11.2026 | Claude | Kod önerisi — Hafta 7 | Önceki kurallara uyarak (öğrenci seviyesi kod, doğru cevap öğrenciye gitmesin) 7. haftayı hazırla. | Öğretmen ve öğrenci olarak iki ayrı pencereyle oturum açma, katılma ve soru geçişini deneyeceğim. (doldur) | __ |
| 12 | 05.11.2026 | Claude | Dokümantasyon | Hafta 7 notları, haftalık ilerleme raporu ve AI günlüğünü PDF şablonlarına göre hazırla. | Metinleri okuyup kendi yaptıklarımla karşılaştırdım; öğrenci bilgilerini ve eksik alanları kendim doldurdum. | E |

**Not:** 11. satırdaki doğrulama, Hafta 7 demosu kendi bilgisayarımda denendikten sonra doldurulacak. Projede gerçek kişisel veri yapay zekâya verilmedi; tüm test verileri sentetiktir (KVKK).
