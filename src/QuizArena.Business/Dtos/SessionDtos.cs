using QuizArena.Entities.Enums;

namespace QuizArena.Business.Dtos;

// ===== Listeler =====

public class SessionListDto
{
    public int Id { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MyNickname { get; set; } // öğrenci listesinde kendi takma adı
}

public class ParticipantDto
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public int TotalScore { get; set; }
}

// Ekranda gösterilen soru.
// DİKKAT: Burada doğru cevap (IsCorrect) bilerek YOK, öğrenciye doğru cevap gitmesin.
public class LiveQuestionDto
{
    public int OrderNo { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public int Points { get; set; }
    public List<string> Options { get; set; } = new();
}

// ===== Öğretmen ekranı =====

public class SessionHostDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int QuestionCount { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
    public LiveQuestionDto? CurrentQuestion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

// ===== Öğrenci ekranı =====

public class StudentSessionDto
{
    public int SessionId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int QuestionCount { get; set; }
    public string MyNickname { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public LiveQuestionDto? CurrentQuestion { get; set; }
}

// ===== Formdan gelen =====

public class JoinSessionDto
{
    public string JoinCode { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
}
