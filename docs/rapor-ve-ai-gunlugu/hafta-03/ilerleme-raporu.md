# Haftalık İlerleme Raporu — Hafta 3

> Şablon 9 · Yazdırılabilir sürüm: `Hafta-03-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | Tarih |
|---|---|---|---|
| Hamza Önver | 251161043 | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) | 15.10.2026 |

**Hafta No:** 3 · Kurulum, Mimari ve Veri Modeli

## Bu hafta tamamlanan işler

- Teknoloji yığınını seçtim: C# / .NET 8, ASP.NET Core MVC, Entity Framework Core, SQLite.
- Katmanlı proje iskeletini kurdum: QuizArena.Entities, Data, Business, Web (4 proje, tek çözüm).
- GitHub deposunu açtım; README ve .gitignore ekledim (bin/, obj/, *.db, .env depoya gitmiyor).
- ER diyagramını hazırladım: kullanıcı (öğretmen/öğrenci), quiz, soru, şık, oturum, katılımcı, cevap.
- Mimari şemayı çizdim (docs/mimari.png).

## Karşılaşılan sorunlar

- Visual Studio'da projeyi çalıştırınca "başlangıç projesi" hatası aldım (sınıf kitaplığı çalıştırılmaya çalışılıyordu).
- İlk üretilen kod bir öğrenci projesi için fazla karmaşıktı, sunumda anlatmam zor olacaktı.

## Uygulanan çözümler

- QuizArena.Web projesini başlangıç projesi (Set as Startup Project) yaptım.
- Kodu sadeleştirdim: servis arayüzlerini kaldırdım, DTO'ları basit sınıflara çevirdim, Türkçe açıklama ekledim.

## Yapay zekâ kullanımı özeti (günlüğe atıf)

Mimari ve klasör yapısı önerisi, ER varlık/ilişki önerisi ve hata ayıklama için kullandım (Bu haftanın AI günlüğü, 1–6. satırlar). → [ai-kullanim-gunlugu.md](ai-kullanim-gunlugu.md)

## Sonraki hafta planı

Veritabanı tablolarını migration ile oluşturmak, ilişkileri/indeksleri kurmak, sentetik veri ve CRUD.

## Ekler

- Depo: https://github.com/hamzaonver/QuizYarismasi
- ER diyagramı: docs/er-diyagrami.png
- Mimari şema: docs/mimari.png
