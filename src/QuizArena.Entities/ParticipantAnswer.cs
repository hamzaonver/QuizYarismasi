namespace QuizArena.Entities;

// Katılımcının bir soruya verdiği cevap (Katılımcı ile Soru arasındaki N-N ara tablosu)
public class ParticipantAnswer : BaseEntity
{
    public int ParticipantId { get; set; }      // FK -> Participants
    public Participant Participant { get; set; } = null!;

    public int QuestionId { get; set; }         // FK -> Questions
    public Question Question { get; set; } = null!;

    public int? SelectedOptionId { get; set; }  // FK -> AnswerOptions (süre biterse null)
    public AnswerOption? SelectedOption { get; set; }

    public bool IsCorrect { get; set; }
    public int ResponseTimeMs { get; set; }     // Ne kadar sürede cevapladı (puan hesabı için)
    public int PointsAwarded { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}
