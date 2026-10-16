# Hafta 4 — Veritabanı Tasarımı ve Veri Erişim Katmanı

**Amaç:** Veritabanı şemasını oluşturmak, ilişkileri kurmak ve temel veri erişim katmanını yazmak.

## Yapılanlar

- [x] Tablolar: EF Core **migration** ile (`dotnet ef migrations add InitialCreate`) + okunabilir SQL eşdeğeri [`../database/schema.sql`](../database/schema.sql)
- [x] İlişkiler, kısıtlar ve indeksler: `src/QuizArena.Data/QuizArenaDbContext.cs` → `OnModelCreating` (Fluent API)
  - FK + silme davranışı (Cascade / Restrict / SetNull)
  - UNIQUE: `Users.Email`, `QuizSessions.JoinCode`, `(SessionId, Nickname)`, `(ParticipantId, QuestionId)`
  - CHECK: rol, süre (5‑300 sn), puan (0‑5000), sıra ≥ 1
- [x] Sentetik veriyle doldurma: `src/QuizArena.Data/Seed/DbInitializer.cs` (2 öğretmen, 4 öğrenci, 3 quiz, 1 tamamlanmış oturum + cevaplar)
- [x] Temel CRUD: `IRepository<T>` / `Repository<T>` + özel depolar (`UserRepository`, `QuizRepository`, `QuestionRepository`)
- [x] CRUD'un SQL karşılıkları: [`../database/crud-ornekleri.sql`](../database/crud-ornekleri.sql)

## Teslim edilecekler

| Çıktı | Nerede |
|---|---|
| Veritabanı şeması / betikleri | `src/QuizArena.Data/Migrations/` (sen oluşturacaksın), `docs/database/schema.sql`, `docs/database/migration.sql` |
| Örnek veriyle dolu veritabanı | Uygulama ilk açılışta `quizarena.db` oluşturur ve doldurur |
| CRUD veri erişim kodu | `src/QuizArena.Data/Repositories/` |

## Gösterilecek ilerleme (demo)

1. `dotnet run --project src/QuizArena.Web` → konsolda EF Core'un çalıştırdığı `CREATE TABLE` / `INSERT` SQL'leri görünür.
2. `quizarena.db` dosyasını **DB Browser for SQLite** ile aç → tabloları, FK'leri ve örnek veriyi göster.
3. Örnek sorgu: `docs/database/crud-ornekleri.sql` içindeki JOIN sorgusunu çalıştır.
4. Kısıt denemesi: aynı e‑postayla ikinci kullanıcı eklemeye çalış → `UNIQUE constraint failed`.

## Kontrol kriterleri

- [x] İlişki ve kısıtlar çalışıyor → FK/UNIQUE/CHECK denemeleri hata veriyor
- [x] CRUD işlemleri sorunsuz → quiz/soru ekle-oku-güncelle-sil arayüzden de çalışıyor (Hafta 6)
- [x] Sentetik veri KVKK'ya uygun → tüm ad/e‑postalar "Demo …", "@quizarena.test"; gerçek kişi verisi yok

## Senin kendi geliştirmen gereken kısımlar

- Migration'ı **kendin** oluştur (`scripts/ilk-kurulum.ps1` ya da README'deki komut) ve oluşan `Migrations/..._InitialCreate.cs` dosyasını oku: `CreateTable`, `ForeignKey`, `CreateIndex` satırlarını `QuizArenaDbContext.OnModelCreating` içindeki ayarlarla eşleştir.
- "En az bir ilişki ve kısıtı elle yazıp açıklamak" → `QuizArenaDbContext.cs` içindeki Participants bölümünde N‑N ilişkiyi ve UNIQUE `(SessionId, Nickname)` kısıtını kendi cümlelerinle anlatabilmelisin. İstersen kendi eklediğin alana (Hafta 3) bir CHECK kısıtı yaz ve ikinci bir migration oluştur: `dotnet ef migrations add KategoriEklendi ...`.

## Sözlü sınav soruları — cevap taslakları

**1) Bir tabloya neden indeks ekledin/eklemedin?**
`Users.Email`'e UNIQUE indeks ekledim: her girişte e‑postayla arama yapılıyor ve aynı e‑posta iki kez olmamalı. `Quizzes.TeacherId`'ye indeks ekledim çünkü "öğretmenin quizleri" sayfası her açılışta bu sütuna göre filtreliyor. Her sütuna indeks eklemedim: indeksler okumayı hızlandırır ama her INSERT/UPDATE'te güncellenmeleri gerektiği için yazmayı yavaşlatır ve yer kaplar. `Questions (QuizId, OrderNo)` indeksini bilerek UNIQUE yapmadım: soru silinip sıra yeniden numaralanırken ara adımda iki soru aynı numarayı alabiliyor; sıra tutarlılığını iş katmanı sağlıyor.

**2) Yabancı anahtar kısıtı ne işe yarar?**
Referans bütünlüğünü korur: var olmayan bir quize soru eklenemez (`FOREIGN KEY constraint failed`). Silme davranışını da belirler: quiz silinince soruları otomatik silinir (Cascade); ama oturum geçmişi olan quiz silinemez (Restrict), böylece eski sonuç raporları bozulmaz.

**3) N‑N ilişkiyi nasıl modelledin?**
İki tane var. Oturum ↔ Kullanıcı ilişkisini `Participants` ara tablosuyla kurdum (SessionId FK + UserId FK). Ara tabloya takma ad ve toplam puan da koymam gerektiği için onu ayrı bir varlık yaptım. Aynı şekilde Katılımcı ↔ Soru ilişkisi `ParticipantAnswers` tablosu; orada seçilen şık, cevap süresi ve kazanılan puan tutuluyor. `(ParticipantId, QuestionId)` UNIQUE: bir soru iki kez cevaplanamaz.

**4) Migration neden kullanılır?**
Veritabanı şemasının sürüm kontrolüdür. Şema değişiklikleri kod olarak (C# dosyası) depoda saklanır; ekipteki herkes `dotnet ef database update` ile aynı şemaya ulaşır. Elle `ALTER TABLE` yazmaya gerek kalmaz, değişiklik geri de alınabilir (`Down` metodu).

**5) Bir CRUD işleminin SQL'ini açıkla.**
Örneğin quiz güncelleme: `UPDATE Quizzes SET Title = @title, UpdatedAt = @now WHERE Id = @id`. Uygulamada önce `SELECT` ile quiz okunuyor, sahibinin giriş yapan öğretmen olduğu kontrol ediliyor, sonra özellikler değiştirilip `SaveAsync` çağrılıyor; EF Core sadece değişen sütunlar için UPDATE üretiyor. Parametreli sorgu kullanıldığı için SQL enjeksiyonu mümkün değil.

## Teorik kavramlar (kısa)

- **DDL** (CREATE/ALTER/DROP: yapı) / **DML** (INSERT/SELECT/UPDATE/DELETE: veri)
- **İndeks:** okuma hızı ↔ yazma maliyeti
- **Referans bütünlüğü:** FK'nin işaret ettiği kayıt var olmalı
- **İşlem (transaction):** `SaveChangesAsync` (bizim `SaveAsync` metodumuz bunu çağırıyor) içindeki tüm komutlar ya hep birlikte kaydedilir ya hiçbiri (atomiklik)
