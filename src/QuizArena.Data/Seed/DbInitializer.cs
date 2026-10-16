using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;
using QuizArena.Entities.Enums;

namespace QuizArena.Data.Seed;

// Uygulama açılırken veritabanını hazırlar ve boşsa örnek veri ekler.
// Örnek verilerin hepsi uydurma (KVKK: gerçek kişi bilgisi yok).
public static class DbInitializer
{
    public static async Task InitializeAsync(QuizArenaDbContext db, Func<string, string> hashPassword, bool seedDemoData)
    {
        // Migration varsa uygula, yoksa tabloları direkt oluştur
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // Örnek veri istenmiyorsa ya da zaten kullanıcı varsa bir şey yapma
        if (!seedDemoData || await db.Users.AnyAsync())
            return;

        // ----- Kullanıcılar -----
        var teacher1 = new User { FullName = "Demo Öğretmen", Email = "ogretmen@quizarena.test", PasswordHash = hashPassword("Ogretmen123!"), Role = UserRole.Teacher };
        var teacher2 = new User { FullName = "Demo Öğretmen 2", Email = "ogretmen2@quizarena.test", PasswordHash = hashPassword("Ogretmen123!"), Role = UserRole.Teacher };
        db.Users.Add(teacher1);
        db.Users.Add(teacher2);

        string studentHash = hashPassword("Ogrenci123!");
        var students = new List<User>();
        for (int i = 1; i <= 4; i++)
        {
            var s = new User { FullName = "Demo Öğrenci " + i, Email = "ogrenci" + i + "@quizarena.test", PasswordHash = studentHash, Role = UserRole.Student };
            students.Add(s);
            db.Users.Add(s);
        }

        // ----- Quizler -----
        var hardwareQuiz = new Quiz
        {
            Teacher = teacher1,
            Title = "Bilgisayar Donanımı Temelleri",
            Description = "İşlemci, bellek ve depolama birimleri üzerine kısa tekrar.",
            DefaultTimeLimitSeconds = 20,
            IsPublished = true
        };
        hardwareQuiz.Questions.Add(CreateQuestion(1, "Bilgisayarın 'beyni' olarak bilinen bileşen hangisidir?", 20, 0, "CPU (İşlemci)", "RAM", "SSD", "Güç kaynağı"));
        hardwareQuiz.Questions.Add(CreateQuestion(2, "Elektrik kesildiğinde içeriği silinen bellek türü hangisidir?", 20, 1, "ROM", "RAM", "HDD", "Flash bellek"));
        hardwareQuiz.Questions.Add(CreateQuestion(3, "Aşağıdakilerden hangisi bir giriş birimidir?", 15, 2, "Monitör", "Yazıcı", "Klavye", "Hoparlör"));
        hardwareQuiz.Questions.Add(CreateQuestion(4, "1 Bayt kaç bitten oluşur?", 10, 3, "2", "4", "16", "8"));

        var csharpQuiz = new Quiz
        {
            Teacher = teacher1,
            Title = "C# Temelleri",
            Description = "Değişkenler, veri tipleri ve döngüler.",
            DefaultTimeLimitSeconds = 30,
            IsPublished = false
        };
        csharpQuiz.Questions.Add(CreateQuestion(1, "C#'ta tam sayı tutmak için hangi tip kullanılır?", 30, 0, "int", "string", "bool"));
        csharpQuiz.Questions.Add(CreateQuestion(2, "Hangisi bir döngü yapısı değildir?", 30, 2, "for", "while", "switch", "foreach"));
        csharpQuiz.Questions.Add(CreateQuestion(3, "'bool' tipi hangi değerleri alabilir?", 20, 1, "0-255", "true / false"));

        var networkQuiz = new Quiz
        {
            Teacher = teacher2,
            Title = "Ağ Temelleri",
            DefaultTimeLimitSeconds = 20,
            IsPublished = true
        };
        networkQuiz.Questions.Add(CreateQuestion(1, "IP adresi ne işe yarar?", 20, 0, "Cihazı ağda tanımlar", "Dosya sıkıştırır", "Ekranı yeniler"));
        networkQuiz.Questions.Add(CreateQuestion(2, "HTTP'nin varsayılan port numarası hangisidir?", 20, 1, "21", "80", "443", "3306"));

        db.Quizzes.Add(hardwareQuiz);
        db.Quizzes.Add(csharpQuiz);
        db.Quizzes.Add(networkQuiz);

        // ----- Bitmiş örnek oturum (sonuç tablolarını göstermek için) -----
        var session = new QuizSession
        {
            Quiz = hardwareQuiz,
            Host = teacher1,
            JoinCode = "DEMO01",
            Status = SessionStatus.Finished,
            CurrentQuestionIndex = 3,
            StartedAt = DateTime.UtcNow.AddDays(-1),
            EndedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(5)
        };
        db.QuizSessions.Add(session);

        var participants = new List<Participant>();
        for (int i = 0; i < 3; i++)
        {
            participants.Add(new Participant { Session = session, User = students[i], Nickname = "Öğrenci " + (i + 1) });
        }
        participants.Add(new Participant { Session = session, Nickname = "Konuk42" }); // misafir

        // Rastgele ama her seferinde aynı çıkan cevaplar (seed = 42)
        var random = new Random(42);
        foreach (var p in participants)
        {
            foreach (var q in hardwareQuiz.Questions)
            {
                AnswerOption selected;
                if (random.Next(100) < 70)
                    selected = q.Options.First(o => o.IsCorrect);   // %70 ihtimalle doğru
                else
                    selected = q.Options[random.Next(q.Options.Count)];

                int timeMs = random.Next(1500, q.TimeLimitSeconds * 1000);

                // Geçici puan hesabı: hızlı cevap daha çok puan (Hafta 11'de düzenlenecek)
                int points = 0;
                if (selected.IsCorrect)
                {
                    double ratio = (double)timeMs / (q.TimeLimitSeconds * 1000);
                    points = (int)(q.Points * (1 - ratio / 2));
                }

                p.Answers.Add(new ParticipantAnswer
                {
                    Question = q,
                    SelectedOption = selected,
                    IsCorrect = selected.IsCorrect,
                    ResponseTimeMs = timeMs,
                    PointsAwarded = points
                });
                p.TotalScore += points;
            }
            db.Participants.Add(p);
        }

        await db.SaveChangesAsync();
    }

    // Yardımcı metot: soru ve şıklarını oluşturur. correctIndex = doğru şıkkın sırası (0'dan başlar)
    private static Question CreateQuestion(int order, string text, int seconds, int correctIndex, params string[] options)
    {
        var question = new Question
        {
            OrderNo = order,
            Text = text,
            TimeLimitSeconds = seconds,
            Points = 1000
        };

        for (int i = 0; i < options.Length; i++)
        {
            question.Options.Add(new AnswerOption
            {
                Text = options[i],
                OrderNo = i + 1,
                IsCorrect = (i == correctIndex)
            });
        }

        return question;
    }
}
