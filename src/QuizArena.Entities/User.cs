using QuizArena.Entities.Enums;

namespace QuizArena.Entities;

// Öğretmen ve öğrenciyi aynı tabloda tutuyorum, farkı Role alanı.
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Parolanın kendisi değil, BCrypt ile hash'lenmiş hali saklanıyor
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    // Çok fazla hatalı girişte hesabı kilitlemek için
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public List<Quiz> Quizzes { get; set; } = new();                  // öğretmenin quizleri
    public List<QuizSession> HostedSessions { get; set; } = new();    // öğretmenin açtığı oturumlar
    public List<Participant> Participations { get; set; } = new();    // öğrencinin katıldığı oturumlar
}
