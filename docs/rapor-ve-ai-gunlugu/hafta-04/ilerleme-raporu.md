# Haftalık İlerleme Raporu — Hafta 4

> Şablon 9 · Yazdırılabilir sürüm: `Hafta-04-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | Tarih |
|---|---|---|---|
|  |  | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) | 15.10.2026 |

**Hafta No:** 4 · Veritabanı Tasarımı ve Veri Erişim Katmanı

## Bu hafta tamamlanan işler

- Tabloları EF Core migration ile oluşturdum (InitialCreate). SQL karşılığı: docs/database/migration.sql.
- İlişkileri, kısıtları (UNIQUE, CHECK, FK) ve indeksleri QuizArenaDbContext içinde tanımladım.
- Sentetik veriyle doldurdum (DbInitializer): demo öğretmen/öğrenciler, 3 quiz. Gerçek kişi verisi yok (KVKK).
- Temel CRUD için IRepository<T> / Repository<T> ve özel depolar (User, Quiz, Question) yazıldı.

## Karşılaşılan sorunlar

- Migration'ı ilk kez çalıştırırken komutların nerede ve hangi sırayla yazılacağını bilmiyordum.
- Oluşan veritabanının içini görmek için bir araca ihtiyacım vardı.

## Uygulanan çözümler

- Migration'ı kendi bilgisayarımda oluşturup Update-Database ile veritabanını kurdum.
- DB Browser for SQLite kurup tabloları, ilişkileri ve sentetik veriyi kontrol ettim.

## Yapay zekâ kullanımı özeti (günlüğe atıf)

Şema, indeks önerisi ve migration örnekleri için kullandım (Bu haftanın AI günlüğü, 1–2. satırlar). → [ai-kullanim-gunlugu.md](ai-kullanim-gunlugu.md)

## Sonraki hafta planı

Kayıt/giriş, parola hash'leme ve rol tabanlı erişim (öğretmen/öğrenci).

## Ekler

- Commit: https://github.com/hamzaonver/QuizYarismasi/commit/2341a3a
- Migration: src/QuizArena.Data/Migrations/
- SQL: docs/database/schema.sql, crud-ornekleri.sql
