namespace QuizArena.Entities;

public class Question : BaseEntity
{
    public int QuizId { get; set; }             // FK -> Quizzes
    public Quiz Quiz { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public int OrderNo { get; set; }            // Quiz içindeki sırası (1, 2, 3...)
    public int TimeLimitSeconds { get; set; } = 20;
    public int Points { get; set; } = 1000;

    public List<AnswerOption> Options { get; set; } = new();
    public List<ParticipantAnswer> Answers { get; set; } = new();
}
