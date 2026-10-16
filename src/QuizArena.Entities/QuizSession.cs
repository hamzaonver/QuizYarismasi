using QuizArena.Entities.Enums;

namespace QuizArena.Entities;

// Bir quizin sınıfta canlı oynatılması (Hafta 7)
public class QuizSession : BaseEntity
{
    public int QuizId { get; set; }             // FK -> Quizzes
    public Quiz Quiz { get; set; } = null!;

    public int HostId { get; set; }             // FK -> Users (oturumu açan öğretmen)
    public User Host { get; set; } = null!;

    public string JoinCode { get; set; } = string.Empty;   // Öğrencilerin gireceği kod, örn: K7P2QX
    public SessionStatus Status { get; set; } = SessionStatus.Waiting;
    public int CurrentQuestionIndex { get; set; } = -1;    // -1 = daha başlamadı

    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public List<Participant> Participants { get; set; } = new();
}
