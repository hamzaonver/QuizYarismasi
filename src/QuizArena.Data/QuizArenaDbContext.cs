using Microsoft.EntityFrameworkCore;
using QuizArena.Entities;

namespace QuizArena.Data;

public class QuizArenaDbContext : DbContext
{
    public QuizArenaDbContext(DbContextOptions<QuizArenaDbContext> options) : base(options)
    {
    }

    // Her DbSet veritabanında bir tablo
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
    public DbSet<QuizSession> QuizSessions { get; set; } = null!;
    public DbSet<Participant> Participants { get; set; } = null!;
    public DbSet<ParticipantAnswer> ParticipantAnswers { get; set; } = null!;

    // Tabloların ilişkileri, kısıtları ve indeksleri burada ayarlanıyor (Fluent API)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Users ----------
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t => t.HasCheckConstraint("CK_Users_Role", "\"Role\" IN (1, 2)"));
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(150);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);

            // Aynı e-posta ile iki hesap açılamasın + girişte e-postaya göre arama hızlı olsun
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ---------- Quizzes ----------
        modelBuilder.Entity<Quiz>(e =>
        {
            e.ToTable("Quizzes", t => t.HasCheckConstraint("CK_Quizzes_DefaultTime", "\"DefaultTimeLimitSeconds\" BETWEEN 5 AND 300"));
            e.Property(q => q.Title).IsRequired().HasMaxLength(150);
            e.Property(q => q.Description).HasMaxLength(500);

            // Öğretmen (1) - Quiz (N). Quizi olan öğretmen silinemez.
            e.HasOne(q => q.Teacher)
             .WithMany(u => u.Quizzes)
             .HasForeignKey(q => q.TeacherId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(q => q.TeacherId); // "öğretmenin quizleri" sorgusu için
        });

        // ---------- Questions ----------
        modelBuilder.Entity<Question>(e =>
        {
            e.ToTable("Questions", t =>
            {
                t.HasCheckConstraint("CK_Questions_TimeLimit", "\"TimeLimitSeconds\" BETWEEN 5 AND 300");
                t.HasCheckConstraint("CK_Questions_Points", "\"Points\" BETWEEN 0 AND 5000");
            });
            e.Property(q => q.Text).IsRequired().HasMaxLength(500);

            // Quiz (1) - Soru (N). Quiz silinince soruları da silinir.
            e.HasOne(q => q.Quiz)
             .WithMany(z => z.Questions)
             .HasForeignKey(q => q.QuizId)
             .OnDelete(DeleteBehavior.Cascade);

            // Unique yapmadım çünkü soru silinince sıralar yeniden numaralanıyor
            // ve arada iki soru aynı numarayı alabiliyor.
            e.HasIndex(q => new { q.QuizId, q.OrderNo });
        });

        // ---------- AnswerOptions ----------
        modelBuilder.Entity<AnswerOption>(e =>
        {
            e.ToTable("AnswerOptions");
            e.Property(o => o.Text).IsRequired().HasMaxLength(200);

            // Soru (1) - Şık (N). Soru silinince şıkları da silinir.
            e.HasOne(o => o.Question)
             .WithMany(q => q.Options)
             .HasForeignKey(o => o.QuestionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- QuizSessions ----------
        modelBuilder.Entity<QuizSession>(e =>
        {
            e.ToTable("QuizSessions");
            e.Property(s => s.JoinCode).IsRequired().HasMaxLength(8);

            // Oturumu (sonucu) olan quiz silinemez
            e.HasOne(s => s.Quiz)
             .WithMany(q => q.Sessions)
             .HasForeignKey(s => s.QuizId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Host)
             .WithMany(u => u.HostedSessions)
             .HasForeignKey(s => s.HostId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => s.JoinCode).IsUnique(); // Katılım kodu benzersiz olmalı
        });

        // ---------- Participants (Oturum - Kullanıcı N-N) ----------
        modelBuilder.Entity<Participant>(e =>
        {
            e.ToTable("Participants");
            e.Property(p => p.Nickname).IsRequired().HasMaxLength(30);

            e.HasOne(p => p.Session)
             .WithMany(s => s.Participants)
             .HasForeignKey(p => p.SessionId)
             .OnDelete(DeleteBehavior.Cascade);

            // Kullanıcı silinirse katılım kaydı kalsın, UserId boş olsun
            e.HasOne(p => p.User)
             .WithMany(u => u.Participations)
             .HasForeignKey(p => p.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            // Bir oturumda aynı takma ad iki kere olmasın
            e.HasIndex(p => new { p.SessionId, p.Nickname }).IsUnique();
        });

        // ---------- ParticipantAnswers (Katılımcı - Soru N-N) ----------
        modelBuilder.Entity<ParticipantAnswer>(e =>
        {
            e.ToTable("ParticipantAnswers");

            e.HasOne(a => a.Participant)
             .WithMany(p => p.Answers)
             .HasForeignKey(a => a.ParticipantId)
             .OnDelete(DeleteBehavior.Cascade);

            // Cevabı olan soru silinemesin (sonuçlar bozulmasın)
            e.HasOne(a => a.Question)
             .WithMany(q => q.Answers)
             .HasForeignKey(a => a.QuestionId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.SelectedOption)
             .WithMany()
             .HasForeignKey(a => a.SelectedOptionId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            // Bir katılımcı bir soruyu sadece bir kere cevaplayabilir
            e.HasIndex(a => new { a.ParticipantId, a.QuestionId }).IsUnique();
        });
    }
}
