# Mimari — QuizArena

> Hafta 3 teslimi, Hafta 8 (v0.8.0) ile güncellendi. Görsel hâli: [`mimari.png`](mimari.png) (Hafta 3 hâli).

## Teknoloji yığını

| Katman | Teknoloji | Neden |
|---|---|---|
| Dil / platform | C# · .NET 8 (LTS) | Derslerde C# kullanıyoruz; uzun süreli destekli sürüm |
| Web çatısı | ASP.NET Core MVC | Sunucu tarafında sayfa üretimi, hazır kimlik doğrulama ve yetkilendirme |
| Gerçek zamanlı (Hafta 9) | ASP.NET Core SignalR | .NET ile hazır gelen WebSocket kütüphanesi; bağlantı kopunca yeniden bağlanma desteği |
| ORM | Entity Framework Core 8 | Tabloları C# sınıflarıyla yönetme, migration desteği |
| Veritabanı | SQLite | Kurulum gerektirmez, tek dosya. EF Core sayesinde SQL Server'a geçiş tek satır |
| Parola özeti | BCrypt.Net-Next | Tuzlu (salt) ve yavaş özetleme; kaba kuvvete dayanıklı |
| Arayüz | Razor View + Bootstrap 5 | Duyarlı (mobil uyumlu) tasarım |

## Katmanlı mimari (sunum / iş / veri)

```mermaid
flowchart TB
    U(["Tarayıcı — Öğretmen / Öğrenci"])
    WEB["<b>SUNUM · QuizArena.Web</b><br/>Controller'lar · Razor View'lar · ViewModel'ler<br/>Çerez oturumu · Authorize Roles · CSRF"]
    BUS["<b>İŞ · QuizArena.Business</b><br/>AuthService · QuizService · SessionService<br/>Doğrulama ve iş kuralları · Sahiplik kontrolü<br/>BCrypt parola özeti · DTO'lar"]
    DAT["<b>VERİ · QuizArena.Data</b><br/>Repository'ler · QuizArenaDbContext (EF Core)<br/>İlişki / indeks / CHECK ayarları · Migrations · Seed"]
    DB[("SQLite<br/>quizarena.db")]
    ENT[["<b>QuizArena.Entities</b><br/>User · Quiz · Question · AnswerOption<br/>QuizSession · Participant · ParticipantAnswer"]]

    U -->|"HTTP istek / HTML yanıt"| WEB
    WEB -->|"DTO ile çağırır"| BUS
    BUS -->|"Repository arayüzleri"| DAT
    DAT -->|"SQL"| DB
    ENT -.->|"tüm katmanlar kullanır"| DAT

    classDef web fill:#ede9fe,stroke:#6c2bd9,color:#1f1534
    classDef bus fill:#fce7f3,stroke:#e11d74,color:#1f1534
    classDef dat fill:#dbeafe,stroke:#2563eb,color:#1f1534
    classDef ent fill:#f1f5f9,stroke:#64748b,color:#1f1534
    class WEB web
    class BUS bus
    class DAT dat
    class ENT,DB ent
```

**Bağımlılık yönü tek yönlüdür:** Web → Business → Data → Entities. Alt katman üst katmanı tanımaz (Data, Web'den habersizdir). Böylece örneğin arayüzü ileride bir mobil uygulama ile değiştirsek iş kuralları aynen kalır.

## Katmanların sorumlulukları

| Proje | Sorumluluk | İçermez |
|---|---|---|
| **QuizArena.Entities** | Veritabanı tablolarına karşılık gelen sade sınıflar ve enum'lar | Hiçbir iş mantığı, hiçbir bağımlılık |
| **QuizArena.Data** | `DbContext`, tablo/ilişki/indeks ayarları, repository'ler, migration ve örnek veri | İş kuralları (ör. "en az 2 şık") |
| **QuizArena.Business** | İş kuralları, doğrulama, sahiplik kontrolü, parola özetleme, DTO'lar | HTTP, çerez, HTML |
| **QuizArena.Web** | HTTP istekleri, oturum çerezi, rol kontrolü, sayfalar | Doğrudan SQL / DbContext kullanımı |

## Klasör yapısı

```
QuizArena/
├── QuizArena.sln
├── README.md
├── .gitignore
├── docs/                         ← Dokümantasyon (ER, mimari, haftalık notlar, SQL)
├── CHANGELOG.md                  ← Sürüm geçmişi (Hafta 8)
├── scripts/                      ← İlk kurulum + otomatik testler (hafta7-test, entegrasyon-testi)
└── src/
    ├── QuizArena.Entities/       ← Varlıklar + Enums/
    ├── QuizArena.Data/
    │   ├── QuizArenaDbContext.cs ← Tablolar + ilişki/indeks/kısıt ayarları (Fluent API)
    │   ├── Repositories/         ← IRepository<T>, QuizRepository, SessionRepository ...
    │   ├── Seed/                 ← Sentetik örnek veri
    │   └── Migrations/           ← (dotnet ef ile oluşturulur)
    ├── QuizArena.Business/
    │   ├── Common/               ← Result (başarı/hata taşıyıcı)
    │   ├── Dtos/                 ← Katmanlar arası veri nesneleri
    │   ├── Helpers/              ← PasswordHelper (BCrypt)
    │   ├── Services/             ← AuthService, QuizService, SessionService (Hafta 7)
    │   └── Validation/           ← İş kuralları ve sabitler
    └── QuizArena.Web/
        ├── Controllers/
        ├── Models/               ← ViewModel'ler
        ├── Views/
        └── wwwroot/              ← CSS, JS, Bootstrap
```

**Klasör mantığı:** Önce katmana göre (proje), sonra her katmanın içinde görev türüne göre (Services, Validation, Repositories...) ayrıldı. Yeni bir modül eklerken (ör. Hafta 7 Canlı Oturum) her katmanda ilgili klasöre bir dosya eklenir: `SessionService`, `SessionRepository`, `SessionController`.

## Değerlendirilen alternatifler

| Alternatif | Neden seçilmedi |
|---|---|
| Tek proje (her şey Web içinde) | Küçük projede hızlı ama iş kuralları controller'lara dağılır, test etmek zorlaşır ("her şeyi tek katmanda toplamak" sık yapılan hata) |
| Clean/Onion Architecture | Daha esnek ama bu ölçekte fazla soyutlama; öğrenme yükü yüksek |
| Razor Pages | MVC kadar yaygın değil; controller ayrımı katmanları anlatmayı kolaylaştırıyor |
| Node.js + Express | C# bilgimizi kullanamazdık |
| SQL Server | Her bilgisayarda kurulum gerektiriyor; geliştirme için SQLite yeterli |

## İstek akışı örneği: "Soru ekle"

1. Öğretmen formu gönderir → `POST /Quiz/AddQuestion?quizId=5`
2. ASP.NET Core CSRF token'ını kontrol eder (yoksa 400)
3. `[Authorize(Roles="Teacher")]` → giriş yapmamışsa giriş sayfasına, öğrenciyse "Erişim engellendi"
4. `QuizController` ViewModel'i doğrular, `QuestionInputDto`'ya çevirir
5. `QuizService.AddQuestionAsync` → quiz var mı (404) → bu öğretmenin mi (403) → kilitli mi → `QuizValidator` kuralları
6. `QuestionRepository.AddAsync` + `SaveAsync` → EF Core `INSERT` komutlarını tek işlemde çalıştırır
7. Başarılıysa quiz sayfasına yönlendirme + "Soru eklendi" mesajı

## Modüller ve veri akışı (Hafta 8 · v0.8.0)

| Modül (hafta) | Controller (Web) | Servis (Business) | Repository (Data) | Tablolar |
|---|---|---|---|---|
| Kullanıcı (5) | `AccountController` | `AuthService` | `UserRepository` | `Users` |
| Quiz oluşturma (6) | `QuizController` | `QuizService` + `QuizValidator` | `QuizRepository`, `QuestionRepository` | `Quizzes`, `Questions`, `AnswerOptions` |
| Canlı oturum (7) | `SessionController` (öğretmen), `StudentController` (öğrenci) | `SessionService` | `SessionRepository` (+ `QuizRepository`) | `QuizSessions`, `Participants` |

**Modüller birbirine nasıl bağlı:**
- Giriş yapan kullanıcının **Id'si ve rolü** çerezden okunur (Hafta 5) → Quiz ve Oturum modülleri "bu kayıt bu kullanıcının mı?" kontrolünü bununla yapar.
- Oturum modülü quizi `QuizRepository` üzerinden okur: sadece **yayındaki ve sorusu olan** quizden oturum açılır (Hafta 6 → 7).
- Quiz modülü oturum tablosuna bakar: oturumu olan quizin **soruları kilitlenir**, quiz **silinemez**, açık oturumu varsa **yayından kaldırılamaz** (Hafta 7 → 6, Hafta 8 düzeltmesi).
- Öğrenci, `Participants` tablosu üzerinden oturuma bağlanır (`SessionId` + `UserId`).

