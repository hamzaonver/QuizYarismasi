namespace QuizArena.Entities;

// Oturum ile kullanıcı arasındaki N-N ilişkinin ara tablosu.
// Ara tabloda takma ad ve puan da tuttuğum için ayrı bir sınıf yaptım.
public class Participant : BaseEntity
{
    public int SessionId { get; set; }          // FK -> QuizSessions
    public QuizSession Session { get; set; } = null!;

    public int? UserId { get; set; }            // FK -> Users (misafir katılırsa null)
    public User? User { get; set; }

    public string Nickname { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public List<ParticipantAnswer> Answers { get; set; } = new();
}
