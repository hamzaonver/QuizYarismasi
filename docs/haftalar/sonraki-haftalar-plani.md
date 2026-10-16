# Sonraki Haftalar İçin Hazırlık (7–15)

3–6. haftalarda altyapı, sonraki modüller eklenirken mevcut kodu bozmayacak şekilde kuruldu. Her hafta için neyin hazır olduğu ve ne ekleneceği:

| Hafta | Hazır olanlar | Eklenecekler (öneri) |
|---|---|---|
| **7 · Canlı oturum ve katılım** | `QuizSession`, `Participant` tabloları; `JoinCode` UNIQUE; `SessionStatus` enum; `(SessionId, Nickname)` UNIQUE; `Quiz.IsPublished`; öğrenci panelinde katılım kodu kutusu | `ISessionRepository` + `SessionService` (oturum aç → 6 haneli kod üret, katıl, başlat, sonraki soru, bitir); `SessionController` (öğretmen ekranı) ve `StudentController.Join`; iş kuralları: sadece yayındaki quiz, sadece Waiting durumda katılım, takma ad benzersiz |
| **8 · Vize ara sürüm** | Çalışan uçtan uca akış (kayıt → quiz → oturum) | `git tag v0.1.0`, demo senaryosu, AI günlüğü güncelleme |
| **9 · Gerçek zamanlı (WebSocket)** | ASP.NET Core içinde SignalR hazır (ek paket gerekmez); oturum durum alanları (`CurrentQuestionIndex`, `StartedAt`) | `Hubs/QuizHub.cs`: gruplar = oturum kodu; olaylar `QuestionStarted`, `AnswerReceived`, `LeaderboardUpdated`; istemcide `withAutomaticReconnect()`; **doğru cevap öğrenciye gönderilmez** |
| **10 · AI soru üretimi** | `QuestionInputDto` + `QuizValidator.ValidateQuestion` → AI çıktısı aynı doğrulamadan geçirilebilir | `IQuestionGenerator` arayüzü + LLM API istemcisi; API anahtarı ortam değişkeninde (`AI__ApiKey`); JSON çıktıyı ayrıştır, doğrula, öğretmen onayıyla quize ekle |
| **11 · Puan / lider tablosu / güç kartları** | `ParticipantAnswer.ResponseTimeMs`, `PointsAwarded`; `Participant.TotalScore`; seed'deki geçici puan formülü | `ScoreService` (süre bonuslu formül), `PowerCard` varlığı (ör. 2x puan, süre dondur, 50:50) + migration; cevap + puan aynı transaction'da |
| **12 · Sonuç analizi** | `ParticipantAnswers.QuestionId` indeksi; seed'de tamamlanmış örnek oturum (DEMO01) | Soru bazında doğru oranı, ortalama süre, en zor soru; Chart.js grafikleri; CSV dışa aktarma |
| **13 · Test** | İş katmanı repository arayüzlerine bağlı → sahte (fake) depolarla test edilebilir; `QuizValidator` saf fonksiyon | `tests/QuizArena.Tests` (xUnit): `QuizValidatorTests`, `AuthServiceTests` (kilitleme), `QuizServiceTests` (sahiplik, kilit) |
| **14 · Dokümantasyon / dağıtım** | README, mimari, ER, ortam değişkenleri, `.gitignore` | HTTPS zorunlu çerez, rate limiting, kurulum doğrulaması temiz makinede |
| **15 · Final** | — | `v1.0.0` etiketi, savunma demosu |

## Yeni bir modül eklerken izlenecek kalıp

1. **Entities:** gerekirse yeni varlık / alan
2. **Data:** `QuizArenaDbContext`'e DbSet + `OnModelCreating` ayarı, gerekirse `IXRepository` / `XRepository` → `Program.cs`'te kaydet → `dotnet ef migrations add X ...`
3. **Business:** DTO'lar, `XValidator`, `XService` (sahiplik + iş kuralları) → `Program.cs`'te kaydet
4. **Web:** ViewModel, `XController` (`[Authorize(Roles=…)]`), View'lar
5. **docs:** ER diyagramı + haftalık not + AI günlüğü
