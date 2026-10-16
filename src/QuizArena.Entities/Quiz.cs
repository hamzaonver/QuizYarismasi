namespace QuizArena.Entities;

public class Quiz : BaseEntity
{
    public int TeacherId { get; set; }          // FK -> Users
    public User Teacher { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Yeni soru eklerken varsayılan olarak gelen süre (saniye)
    public int DefaultTimeLimitSeconds { get; set; } = 20;

    // Sadece yayındaki quizlerle oturum başlatılabilecek
    public bool IsPublished { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<Question> Questions { get; set; } = new();
    public List<QuizSession> Sessions { get; set; } = new();
}
